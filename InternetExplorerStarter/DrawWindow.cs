using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InternetExplorerStarter
{
    public class WindowObject : Window
    {
        public int Id;
        public Point Offset;
        public int W;
        public int H;

        public WindowObject()
        {
            this.Loaded += WindowObject_Loaded;
        }

        private void WindowObject_Loaded(object sender, RoutedEventArgs e)
        {
            Window win = sender as Window;

            // Calculate center position
            double x = ((W - win.Width) / 2) + Math.Abs(Offset.X) * (Offset.X < 0 ? -1 : 1);
            double y = ((H - win.Height) / 2) + Math.Abs(Offset.Y) * (Offset.Y < 0 ? -1 : 1); ;
            win.Left = x;
            win.Top = y;
        }
    }


    public class DrawWindow
    {
        private static Application application;
        private static List<WindowObject> windows;

        public DrawWindow()
        {
            application = new Application();
            windows = new List<WindowObject>();
        }


        public void Create(int number, int x, int y, int w, int h)
        {
            var label = new Label()
            {
                Content = string.Format("{0}", number),
                FontSize = 200,
                Foreground = Brushes.Black,
                Background = Brushes.White,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(3),
            };
            var grid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            grid.Children.Add(label);

            var window = new WindowObject()
            {
                ShowInTaskbar = false,
                Content = grid,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Topmost = true,
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                Id = number,
                Width = 122,
                Height = 285,
                Offset = new Point(x, y),
                W = w,
                H = h,
            };
            window.Left = x;
            window.Top = y;
            window.Show();
            windows.Add(window);
        }

        public void Show()
        {
            foreach (WindowObject win in windows)
            {
                win.Show();
            }
        }

        public void Close()
        {
            foreach (WindowObject win in windows)
            {
                win.Hide();
                win.Close();
            }
        }
    }
}
