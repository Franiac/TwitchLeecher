using System;
using System.Windows;
using System.Windows.Interactivity;

namespace TwitchLeecher.Gui.Behaviors
{
    public class CenterWindowBehavior : Behavior<Window>
    {
        private WindowState mainWindowState;

        private int x;
        private int y;

        protected override void OnAttached()
        {
            Window mainWindow = Application.Current.MainWindow;

            this.mainWindowState = mainWindow.WindowState;

            this.x = (int)Math.Round(mainWindow.Left + mainWindow.Width / 2, 0);
            this.y = (int)Math.Round(mainWindow.Top + mainWindow.Height / 2, 0);

            this.AssociatedObject.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = this.AssociatedObject;

            if (this.mainWindowState == WindowState.Maximized)
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                int width = (int)Math.Round(window.Width, 0);
                int height = (int)Math.Round(window.Height, 0);

                window.Left = this.x - width / 2;
                window.Top = this.y - height / 2;
            }
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= Window_Loaded;
        }
    }
}