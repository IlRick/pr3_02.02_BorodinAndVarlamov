using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using Common;
using Newtonsoft.Json;

namespace pr3_02._02_BorodinAndVarlamov
{
    public class Program
    {
        public static List<Leaders> Leaders = new List<Leaders>();
        public static List<ViewModelUserSettings> remoteIPAddress = new List<ViewModelUserSettings>();
        public static List<ViewModelGames> viewModelGames = new List<ViewModelGames>();
        private static int LocalPort = 5001;
        public static int MaxSpeed = 15;
        static void Main(string[] args)
        {
            try
            {
                Thread tRec = new Thread(new ThreadStart(Receiver));
                tRec.Start();

                Thread tTime = new Thread(new ThreadStart(Timer));
                tTime.Start();

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
            }
        }

        private static void Send()
        {
            foreach (ViewModelUserSettings User in remoteIPAddress)
            {
                UdpClient sender = new UdpClient();
                IPEndPoint endPoint = new IPEndPoint(
                    IPAddress.Parse(User.IPAddress),
                    int.Parse(User.Port));
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
                    Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
                }
                finally
                {
                    sender.Close();
                }
            }
        }

        public static void Receiver()
        {
            UdpClient receivingUdpClient = new UdpClient(LocalPort);
            IPEndPoint RemoteIpEndPoint = null;
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Команды сервера: ");
                while (true)
                {
                    byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.UTF8.GetString(receiveBytes);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Получить команду " + returnData.ToString());

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
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
            }
        }

        public static int AddShake()
        {
            ViewModelGames viewModelGamesPlayer = new ViewModelGames();
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

        public static void Timer()
        {
            while (true)
            {
                Thread.Sleep(100);
                List<ViewModelGames> RemoteShake = viewModelGames.FindAll(x => x.ShakesPlayers.GameOver);
                if (RemoteShake.Count > 0)
                {
                    foreach (ViewModelGames DeadShake in RemoteShake)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Отключил пользователя: {remoteIPAddress.Find(x => x.IdShake == DeadShake.IdShake).IPAddress}" +
                            $":{remoteIPAddress.Find(x => x.IdShake == DeadShake.IdShake).Port}");
                        remoteIPAddress.RemoveAll(x => x.IdShake == DeadShake.IdShake);
                    }
                    viewModelGames.RemoveAll(x => x.ShakesPlayers.GameOver);
                }
                foreach (ViewModelUserSettings User in remoteIPAddress)
                {
                    Shakes shakes = viewModelGames.Find(x => x.IdShake == User.IdShake).ShakesPlayers;
                    for (int i = shakes.Points.Count - 1; i >= 0; i--)
                    {
                        if (i != 0)
                        {
                            shakes.Points[i] = shakes.Points[i - 1];
                        }
                        else
                        {
                            int speed = 10 + (int)Math.Round(shakes.Points.Count / 20f);
                            if (speed > MaxSpeed) speed = MaxSpeed;

                            if (shakes.directory == Shakes.Direction.Right)
                            {
                                shakes.Points[i] = new Shakes.Point() { X = shakes.Points[i].X + speed, Y = shakes.Points[i].Y };
                            }
                            else if (shakes.directory == Shakes.Direction.Down)
                            {
                                shakes.Points[i] = new Shakes.Point() { X = shakes.Points[i].X, Y = shakes.Points[i].Y + speed };
                            }
                            else if (shakes.directory == Shakes.Direction.Up)
                            {
                                shakes.Points[i] = new Shakes.Point() { X = shakes.Points[i].X, Y = shakes.Points[i].Y - speed };
                            }
                            else if (shakes.directory == Shakes.Direction.Left)
                            {
                                shakes.Points[i] = new Shakes.Point() { X = shakes.Points[i].X - speed, Y = shakes.Points[i].Y };
                            }
                        }
                    }
                    if (shakes.Points[0].X <= 0 || shakes.Points[0].X >= 793)
                        shakes.GameOver = true;
                    else if (shakes.Points[0].Y <= 0 || shakes.Points[0].Y >= 420)
                        shakes.GameOver = true;

                    if (shakes.directory != Shakes.Direction.Start)
                    {
                        for (int IPoint = 1; IPoint < shakes.Points.Count; IPoint++)
                        {
                            if (shakes.Points[0].X >= shakes.Points[IPoint].X - 1 && shakes.Points[0].X <= shakes.Points[IPoint].X + 1)
                            {
                                if (shakes.Points[0].Y >= shakes.Points[IPoint].Y - 1 && shakes.Points[0].Y <= shakes.Points[IPoint].Y + 1)
                                {
                                    shakes.GameOver = true;
                                    break;
                                }
                            }
                        }
                        if (shakes.Points[0].X >= viewModelGames.Find(x => x.IdShake == User.IdShake).Points.X - 15 &&
                            shakes.Points[0].X <= viewModelGames.Find(x => x.IdShake == User.IdShake).Points.X + 15)
                        {
                            if (shakes.Points[0].Y >= viewModelGames.Find(x => x.IdShake == User.IdShake).Points.Y - 15 &&
                            shakes.Points[0].Y <= viewModelGames.Find(x => x.IdShake == User.IdShake).Points.Y + 15)
                            {
                                viewModelGames.Find(x => x.IdShake == User.IdShake).Points = new Shakes.Point(
                                    new Random().Next(10, 783),
                                    new Random().Next(10, 410));
                                shakes.Points.Add(new Shakes.Point()
                                {
                                    X = shakes.Points[shakes.Points.Count - 1].X,
                                    Y = shakes.Points[shakes.Points.Count - 1].Y
                                });
                                LoadLeaders();
                                Leaders.Add(new Leaders()
                                {
                                    Name = User.Name,
                                    Points = shakes.Points.Count - 3
                                });
                                Leaders = Leaders.OrderByDescending(x => x.Points).ThenBy(x => x.Name).ToList();
                                viewModelGames.Find(x => x.IdShake == User.IdShake).Top =
                                    Leaders.FindIndex(x => x.Points == shakes.Points.Count - 3 && x.Name == User.Name) + 1;
                            }
                        }

                        if (shakes.GameOver)
                        {
                            LoadLeaders();
                            Leaders.Add(new Leaders()
                            {
                                Name = User.Name,
                                Points = shakes.Points.Count - 3
                            });
                            SaveLeaders();
                        }
                    }
                }

                Send();
            }
        }

        private static void LoadLeaders()
        {
            if (File.Exists("./leaders.txt"))
            {
                StreamReader SR = new StreamReader("./leaders.txt");
                string json = SR.ReadLine();
                SR.Close();
                if (!string.IsNullOrEmpty(json))
                    Leaders = JsonConvert.DeserializeObject<List<Leaders>>(json);
                else
                    Leaders = new List<Leaders>();
            }
            else
                Leaders = new List<Leaders>();
        }

        public static void SaveLeaders()
        {
            string json = JsonConvert.SerializeObject(Leaders);
            StreamWriter SW = new StreamWriter("./leaders.txt");
            SW.WriteLine(json);
            SW.Close();
        }



    }
}
