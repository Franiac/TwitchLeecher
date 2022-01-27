using CefSharp;
using System;
using System.Windows.Threading;
using TwitchLeecher.Gui.ViewModels;

namespace TwitchLeecher.Gui.Browser
{
    public class AuthCookieVisitor : ICookieVisitor
    {
        private readonly Dispatcher _dispatcher;
        private readonly LoginViewVM _vm;

        public AuthCookieVisitor(Dispatcher dispatcher, LoginViewVM vm)
        {
            _dispatcher = dispatcher;
            _vm = vm;
        }

        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            if (cookie.Name.Equals("auth-token", StringComparison.OrdinalIgnoreCase))
            {
                string authToken = cookie.Value;

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    void CheckWebsiteAuthToken(string token)
                    {
                        _vm.CheckWebsiteAuthTokenCommand.Execute(token);
                    }

                    if (!_dispatcher.CheckAccess())
                    {
                        _dispatcher.Invoke(() => CheckWebsiteAuthToken(authToken));
                    }
                    else
                    {
                        CheckWebsiteAuthToken(authToken);
                    }
                }
            }

            return true;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}