using System;
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

            Loaded += TwitchConnectView_Loaded;
            webBrowser.Navigating += WebBrowser_Navigating;
        }

        private void TwitchConnectView_Loaded(object sender, RoutedEventArgs e)
        {
            HideScriptErrors(webBrowser);
            webBrowser.Navigate("https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=37v97169hnj8kaoq8fs3hzz8v6jezdj&redirect_uri=http://www.tl.com&scope=user_subscriptions&force_verify=true");
        }

        private void WebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            string urlStr = e.Uri?.OriginalString;

            if (!string.IsNullOrWhiteSpace(urlStr) && urlStr.StartsWith("http://www.tl.com", StringComparison.OrdinalIgnoreCase))
            {
                TwitchConnectViewVM vm = DataContext as TwitchConnectViewVM;
                vm?.NavigatingCommand.Execute(e.Uri);
                e.Cancel = true;
            }
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