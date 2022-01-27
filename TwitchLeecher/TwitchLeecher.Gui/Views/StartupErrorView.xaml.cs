using System.Diagnostics;
using System.Windows;

namespace TwitchLeecher.Gui.Views
{
    public partial class StartupErrorView : Window
    {
        public StartupErrorView()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://aka.ms/vs/17/release/vc_redist.x64.exe");
        }
    }
}