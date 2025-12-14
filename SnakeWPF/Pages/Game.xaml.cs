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
        private List<Point> smoothPlayerPoints = new List<Point>();
        private List<List<Point>> smoothOtherPoints = new List<List<Point>>();
        public int StepCadr = 0;
        public Game()
        {
            InitializeComponent();
        }

        public void CreateUI()
        {
            Dispatcher.Invoke(() =>
            {
                canvas.Children.Clear();
                var playerSnake = MainWindow.mainWindow.ViewModelGames.ShakesPlayers.Points;
                while (smoothPlayerPoints.Count < playerSnake.Count)
                    smoothPlayerPoints.Add(new Point(playerSnake[smoothPlayerPoints.Count].X,
                                                     playerSnake[smoothPlayerPoints.Count].Y));

                for (int i = 0; i < playerSnake.Count; i++)
                {
                    var target = playerSnake[i];
                    var smooth = smoothPlayerPoints[i];

                    smooth.X += (target.X - smooth.X) * 0.37;
                    smooth.Y += (target.Y - smooth.Y) * 0.37;

                    smoothPlayerPoints[i] = smooth;

                    Ellipse ellipse = new Ellipse()
                    {
                        Width = 20,
                        Height = 20,
                        Fill = i == 0
                            ? new SolidColorBrush(Color.FromArgb(255, 0, 127, 14))
                            : new SolidColorBrush(Color.FromArgb(255, 0, 198, 19)),
                        Margin = new Thickness(smooth.X - 10, smooth.Y - 10, 0, 0),
                        Stroke = Brushes.Black
                    };
                    canvas.Children.Add(ellipse);
                }
                var others = MainWindow.mainWindow.AllViewModelGames;
                if (others != null)
                {
                    while (smoothOtherPoints.Count < others.Count)
                        smoothOtherPoints.Add(new List<Point>());

                    for (int p = 0; p < others.Count; p++)
                    {
                        var snake = others[p].ShakesPlayers.Points;
                        var smoothList = smoothOtherPoints[p];

                        while (smoothList.Count < snake.Count)
                            smoothList.Add(new Point(snake[smoothList.Count].X,
                                                     snake[smoothList.Count].Y));

                        for (int i = 0; i < snake.Count; i++)
                        {
                            var target = snake[i];
                            var smooth = smoothList[i];

                            smooth.X += (target.X - smooth.X) * 0.37;
                            smooth.Y += (target.Y - smooth.Y) * 0.37;

                            smoothList[i] = smooth;

                            Ellipse ellipse = new Ellipse()
                            {
                                Width = 20,
                                Height = 20,
                                Margin = new Thickness(smooth.X - 10, smooth.Y - 10, 0, 0),
                                Fill = i == 0
                                    ? new SolidColorBrush(Color.FromArgb(255, 135, 135, 135))
                                    : new SolidColorBrush(Color.FromArgb(255, 160, 160, 160)),
                                Stroke = Brushes.Black
                            };
                            canvas.Children.Add(ellipse);
                        }
                    }
                }
                ImageBrush myBrush = new ImageBrush();
                myBrush.ImageSource = new BitmapImage(new Uri($"pack://application:,,,/Image/apple.png"));
                Ellipse points = new Ellipse()
                {
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(MainWindow.mainWindow.ViewModelGames.Points.X - 20, MainWindow.mainWindow.ViewModelGames.Points.Y - 20, 0, 0),
                    Fill = myBrush
                };
                canvas.Children.Add(points);
            });
        }
    }
}
