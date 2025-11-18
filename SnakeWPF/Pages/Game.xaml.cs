using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Common;
using Newtonsoft.Json.Bson;

namespace SnakeWPF.Pages
{
    /// <summary>
    /// Логика взаимодействия для Game.xaml
    /// </summary>
    public partial class Game : Page
    {
        public int StepCadr = 0;
        public Game()
        {
            InitializeComponent();
        }

        public void CreateUI()
        {
            Dispatcher.Invoke(() =>
            {
                if (StepCadr == 0) StepCadr = 1;
                else StepCadr = 0;
                canvas.Children.Clear();
                for(int iPoint= MainWindow.mainWindow.ViewModelGames.ShakesPlayers.Points.Count-1;iPoint>=0;iPoint--)
                {
                    Shakes.Point ShakesPoint = MainWindow.mainWindow.ViewModelGames.ShakesPlayers.Points[iPoint];
                    if(iPoint!=0)
                    {
                        Shakes.Point NextShakePoint = MainWindow.mainWindow.ViewModelGames.ShakesPlayers.Points[iPoint - 1];
                        if(ShakesPoint.X>NextShakePoint.X||ShakesPoint.X<NextShakePoint.X)
                        {
                            if(iPoint%2==0)
                            {
                                if (StepCadr % 2 == 0)
                                    ShakesPoint.Y -= 1;
                                else
                                    ShakesPoint.Y += 1;
                            }
                            else
                            {
                                if (StepCadr % 2 == 0)
                                    ShakesPoint.Y += 1;
                                else
                                    ShakesPoint.Y -= 1;
                            }
                        }
                        else if(ShakesPoint.Y>NextShakePoint.Y||ShakesPoint.Y<NextShakePoint.Y)
                        {
                            if (iPoint % 2 == 0)
                            {
                                if (StepCadr % 2 == 0)
                                    ShakesPoint.X -= 1;
                                else
                                    ShakesPoint.X += 1;
                            }
                            else
                            {
                                if (StepCadr % 2 == 0)
                                    ShakesPoint.X += 1;
                                else
                                    ShakesPoint.X -= 1;
                            }
                        }
                    }
                    Brush Color;
                    if (iPoint == 0)
                        Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 127, 14));
                    else
                        Color = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 198, 19));
                    Ellipse ellipse = new Ellipse()
                    {
                        Width = 20,
                        Height = 20,
                        Margin= new Thickness(ShakesPoint.X-10,ShakesPoint.Y-10,0,0),
                        Fill=Color,
                        Stroke=Brushes.Black
                    };
                    canvas.Children.Add(ellipse);
                }
                ImageBrush myBrash = new ImageBrush();
                myBrash.ImageSource = new BitmapImage(new Uri($"C:\\Users\\student-a502.PERMAVIAT\\Desktop\\rgh\\pr3_02.02_BorodinAndVarlamov\\SnakeWPF\\Image\\apple.png"));
                Ellipse point = new Ellipse()
                {
                    Width= 40,
                    Height=40,
                    Margin= new Thickness(
                        MainWindow.mainWindow.ViewModelGames.Points.X-20,
                        MainWindow.mainWindow.ViewModelGames.Points.Y-20,0,0),
                    Fill=myBrash
                };
                canvas.Children.Add(point);
            });
        }
    }
}
