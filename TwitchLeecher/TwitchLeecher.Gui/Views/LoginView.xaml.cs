using CefSharp;
using System;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using TwitchLeecher.Gui.Browser;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Views
{
    public partial class LoginView : UserControl, IDisposable
    {
        private LoginViewVM _vm;

        private System.Timers.Timer _authTimer;
        private object _authTimerLock;

        private ICookieManager _cookieManager;
        private AuthCookieVisitor _cookieVisitor;

        public LoginView()
        {
            InitializeComponent();

            Loaded += LoginView_Loaded;
            Unloaded += LoginView_Unloaded;
        }

        private AuthCookieVisitor CookieVisitor
        {
            get
            {
                if (_cookieVisitor == null)
                {
                    _cookieVisitor = new AuthCookieVisitor(Dispatcher, _vm);
                }

                return _cookieVisitor;
            }
        }

        public void Dispose()
        {
            if (_authTimer != null)
            {
                _authTimer.Dispose();
            }

            chrome.Dispose();
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            chrome.IsBrowserInitializedChanged += Chrome_IsBrowserInitializedChanged;

            _vm = DataContext as LoginViewVM;

            if (!_vm.SubOnly)
            {
                chrome.RequestHandler = new AuthRequestHandler(Dispatcher, _vm);
            }
        }

        private void AuthTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Monitor.TryEnter(_authTimerLock))
            {
                try
                {
                    _ = _cookieManager.VisitUrlCookies("https://twitch.tv", false, CookieVisitor);
                }
                finally
                {
                    Monitor.Exit(_authTimerLock);
                }
            }
        }

        private void LoginView_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        private void Chrome_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((bool)e.NewValue) == true && _vm != null)
            {
                if (_vm.SubOnly)
                {
                    _cookieManager = chrome.GetCookieManager();

                    _ = _cookieManager.VisitUrlCookies("https://twitch.tv", false, new DeleteCookieVisitor());

                    _authTimerLock = new object();

                    _authTimer = new System.Timers.Timer(2000);
                    _authTimer.Elapsed += AuthTimer_Elapsed;
                    _authTimer.Start();
                }

                chrome.Load(_vm.GetLoginUrl());
            }
        }
    }
}