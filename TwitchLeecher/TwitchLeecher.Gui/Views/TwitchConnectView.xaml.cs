using System.Windows;
using System.Windows.Controls;
using TwitchLeecher.Gui.Browser;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Views
{
    public partial class TwitchConnectView : UserControl
    {
        public TwitchConnectView()
        {
            InitializeComponent();

            Loaded += TwitchConnectView_Loaded;
            Unloaded += TwitchConnectView_Unloaded;
        }

        private void TwitchConnectView_Loaded(object sender, RoutedEventArgs e)
        {
            chrome.IsBrowserInitializedChanged += Chrome_IsBrowserInitializedChanged;
            chrome.RequestHandler = new AuthRequestHandler(Dispatcher, DataContext as TwitchConnectViewVM);
        }

        private void TwitchConnectView_Unloaded(object sender, RoutedEventArgs e)
        {
            chrome.Dispose();
        }

        private void Chrome_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((bool)e.NewValue) == true)
            {
                chrome.Load("https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=37v97169hnj8kaoq8fs3hzz8v6jezdj&redirect_uri=http://www.tl.com&scope=user_subscriptions&force_verify=true");
            }
        }
    }
}