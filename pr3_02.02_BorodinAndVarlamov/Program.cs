using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Common;
using Newtonsoft.Json;

namespace pr3_02._02_BorodinAndVarlamov
{
    public class Program
    {
        public static List<Leaders> Leaders = new List<Leaders>();
        public static List<ViewModelUserSettings> remoteIPAddress = new List<ViewModelUserSettings>();
        public static List<ViewModelGames> viewModelGames = new List<ViewModelGames>();
        private static int LocalPort=5001;
        public static int MaxSpeed = 15;
        static void Main(string[] args)
        {
        }

        private static void Send()
        {
            foreach(ViewModelUserSettings User in remoteIPAddress)
            {
                UdpClient sender =  new UdpClient();
                IPEndPoint endPoint = new IPEndPoint(
                    IPAddress.Parse(User.IPAddress),
                    int.Parse(User.Port) );
                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(viewModelGames.Find(x => x.IdShake == User.IdShake)));
                    sender.Send(bytes, bytes.Length, endPoint);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Отправил данные пользователя:  {User.IPAddress}:{User.Port}");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n "+ ex.Message);
                }
                finally
                {
                    sender.Close();
                }
            }
        }

        public static void Receiver()
        {
            UdpClient receivingUdpClient= new UdpClient();
            IPEndPoint RemoteIpEndPoint= null;
            try
            {
                while (true)
                {
                    byte[] receiveBytes=  receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    string returnData= Encoding.UTF8.GetString(receiveBytes);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Получить команду " +returnData.ToString());

                    if (returnData.ToString().Contains("/start"))
                    {
                        string[] dataMessage = returnData.ToString().Split('|');
                        ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Подключился пользователь: {viewModelUserSettings.IPAddress}:{viewModelUserSettings.Port}");
                        remoteIPAddress.Add(viewModelUserSettings);
                        viewModelUserSettings.IdShake = AddShake();
                        viewModelGames[viewModelUserSettings.IdShake].IdShake = viewModelUserSettings.IdShake;
                    }
                    else
                    {
                        string[] dataMessage = returnData.ToString().Split('|');
                        ViewModelUserSettings viewModelUserSettings = JsonConvert.DeserializeObject<ViewModelUserSettings>(dataMessage[1]);
                        int IdPlayer = -1;
                        IdPlayer = remoteIPAddress.FindIndex(x => x.IPAddress == viewModelUserSettings.IPAddress && x.Port == viewModelUserSettings.Port);
                        if (IdPlayer != -1)
                        {
                            if (dataMessage[0] == "Up" && viewModelGames[IdPlayer].ShakesPlayers.directory != Shakes.Direction.Down)
                                viewModelGames[IdPlayer].ShakesPlayers.directory = Shakes.Direction.Up;
                            else if (dataMessage[0] == "Down" && viewModelGames[IdPlayer].ShakesPlayers.directory != Shakes.Direction.Up)
                                viewModelGames[IdPlayer].ShakesPlayers.directory = Shakes.Direction.Down;
                            else if (dataMessage[0] == "Left" && viewModelGames[IdPlayer].ShakesPlayers.directory != Shakes.Direction.Right)
                                viewModelGames[IdPlayer].ShakesPlayers.directory = Shakes.Direction.Left;
                            else if (dataMessage[0] == "Right" && viewModelGames[IdPlayer].ShakesPlayers.directory != Shakes.Direction.Left)
                                viewModelGames[IdPlayer].ShakesPlayers.directory = Shakes.Direction.Right;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
            }
        }

        public static int AddShake()
        {
            ViewModelGames viewModelGamesPlayer= new ViewModelGames();
            viewModelGamesPlayer.ShakesPlayers = new Shakes()
            {
                Points = new List<Shakes.Point>()
                {
                    new Shakes.Point(){X=30,Y=10},
                    new Shakes.Point(){X=20,Y=10},
                    new Shakes.Point(){X=10,Y=10},
                },
                directory = Shakes.Direction.Start
            };
            viewModelGamesPlayer.Points = new Shakes.Point(new Random().Next(10, 783), new Random().Next(10, 410));
            viewModelGames.Add(viewModelGamesPlayer);
            return viewModelGames.FindIndex(x => x == viewModelGamesPlayer);
        }
    }
}
