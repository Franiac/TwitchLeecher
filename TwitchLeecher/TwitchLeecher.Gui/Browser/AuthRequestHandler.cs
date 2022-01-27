using CefSharp;
using CefSharp.Handler;
using System;
using System.Windows.Threading;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Browser
{
    internal class AuthRequestHandler : RequestHandler
    {
        private readonly Dispatcher _dispatcher;
        private readonly LoginViewVM _vm;

        public AuthRequestHandler(Dispatcher dispatcher, LoginViewVM vm)
        {
            _dispatcher = dispatcher;
            _vm = vm;
        }

        public LoginViewVM DataContext { get; private set; }

        protected override bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            string urlStr = request.Url;

            if (!string.IsNullOrWhiteSpace(urlStr) && urlStr.StartsWith("http://www.tl.com", StringComparison.OrdinalIgnoreCase) && Uri.TryCreate(urlStr, UriKind.Absolute, out Uri accessUri))
            {
                void Navigate(Uri uri)
                {
                    _vm?.CheckRedirectUrlCommand.Execute(uri);
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