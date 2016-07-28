using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Views
{
    public partial class TwitchConnectView : UserControl
    {
        public TwitchConnectView()
        {
            InitializeComponent();

            this.Loaded += this.TwitchConnectView_Loaded;
            this.webBrowser.Navigated += this.WebBrowser_Navigated;
        }

        private void TwitchConnectView_Loaded(object sender, RoutedEventArgs e)
        {
            this.HideScriptErrors(this.webBrowser);
            this.webBrowser.Navigate("https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=37v97169hnj8kaoq8fs3hzz8v6jezdj&redirect_uri=http://www.tl.com&scope=user_subscriptions&force_verify=true");
        }

        private void WebBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            TwitchConnectViewVM vm = this.DataContext as TwitchConnectViewVM;
            vm?.NavigatedCommand.Execute(e.Uri);
        }

        private void HideScriptErrors(WebBrowser wb)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);

            if (fiComWebBrowser == null)
            {
                return;
            }

            object objComWebBrowser = fiComWebBrowser.GetValue(wb);

            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { true });
        }
    }
}