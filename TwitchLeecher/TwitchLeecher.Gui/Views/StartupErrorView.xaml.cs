using System.Diagnostics;
using System.Windows;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Gui.Views
{
    public partial class StartupErrorView : Window
    {
        public StartupErrorView()
        {
            InitializeComponent();

            AssemblyUtil au = AssemblyUtil.Get;

            Title = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://aka.ms/vs/17/release/vc_redist.x64.exe");
        }
    }
}