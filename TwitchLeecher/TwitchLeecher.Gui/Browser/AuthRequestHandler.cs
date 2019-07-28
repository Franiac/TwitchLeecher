using CefSharp;
using CefSharp.Handler;
using System;
using System.Windows.Threading;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Browser
{
    internal class AuthRequestHandler : DefaultRequestHandler
    {
        private Dispatcher _dispatcher;
        private TwitchConnectViewVM _vm;

        public AuthRequestHandler(Dispatcher dispatcher, TwitchConnectViewVM vm)
        {
            _dispatcher = dispatcher;
            _vm = vm;
        }

        public TwitchConnectViewVM DataContext { get; private set; }

        public override bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            string urlStr = request.Url;

            if (!string.IsNullOrWhiteSpace(urlStr) && urlStr.StartsWith("http://www.tl.com", StringComparison.OrdinalIgnoreCase) && Uri.TryCreate(urlStr, UriKind.Absolute, out Uri accessUri))
            {
                void Navigate(Uri uri)
                {
                    _vm?.NavigatingCommand.Execute(uri);
                }

                if (!_dispatcher.CheckAccess())
                {
                    _dispatcher.Invoke(() => Navigate(accessUri));
                }
                else
                {
                    Navigate(accessUri);
                }
            }

            return base.OnBeforeBrowse(browserControl, browser, frame, request, userGesture, isRedirect);
        }
    }
}