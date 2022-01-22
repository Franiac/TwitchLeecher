using System.Windows;
using System.Windows.Controls;
using TwitchLeecher.Gui.Browser;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();

            Loaded += LoginView_Loaded;
            Unloaded += LoginView_Unloaded;
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            chrome.IsBrowserInitializedChanged += Chrome_IsBrowserInitializedChanged;
            chrome.RequestHandler = new AuthRequestHandler(Dispatcher, DataContext as LoginViewVM);
        }

        private void LoginView_Unloaded(object sender, RoutedEventArgs e)
        {
            chrome.Dispose();
        }

        private void Chrome_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((bool)e.NewValue) == true && DataContext != null && DataContext is LoginViewVM vm)
            {
                chrome.Load(vm.GetLoginUrl());
            }
        }
    }
}