using System;
using System.Windows;

namespace TwitchLeecher.Gui.Views
{
    public partial class UpdateInfoView : Window
    {
        public UpdateInfoView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Window mainWindow = Application.Current.MainWindow;

            WindowState mainWindowState = mainWindow.WindowState;

            if (mainWindowState == WindowState.Maximized)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                int x = (int)Math.Round(mainWindow.Left + mainWindow.Width / 2, 0);
                int y = (int)Math.Round(mainWindow.Top + mainWindow.Height / 2, 0);

                int width = (int)Math.Round(Width, 0);
                int height = (int)Math.Round(Height, 0);

                Left = x - width / 2;
                Top = y - height / 2;
            }
        }
    }
}