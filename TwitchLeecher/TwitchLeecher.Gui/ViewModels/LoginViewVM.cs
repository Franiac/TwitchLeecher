using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Web;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Constants;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;
using TwitchLeecher.Shared.Extensions;

namespace TwitchLeecher.Gui.ViewModels
{
    public class LoginViewVM : ViewModelBase
    {
        #region Fields

        private readonly IEventAggregator _eventAggregator;
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;

        private readonly object _commandLockObject;
        private readonly object _checkAuthTokenLockObject;

        private bool _subOnly;

        private ICommand _checkWebsiteAuthTokenCommand;
        private ICommand _checkRedirectUrlCommand;
        private ICommand _cancelCommand;

        #endregion Fields

        #region Constructor

        public LoginViewVM(
            IEventAggregator eventAggregator,
            IAuthService authService,
            IDialogService dialogService,
            INotificationService notificationService)
        {
            _eventAggregator = eventAggregator;
            _authService = authService;
            _dialogService = dialogService;
            _notificationService = notificationService;

            _commandLockObject = new object();
            _checkAuthTokenLockObject = new object();
        }

        #endregion Constructor

        #region Properties

        public bool SubOnly
        {
            get
            {
                return _subOnly;
            }
            set
            {
                SetProperty(ref _subOnly, value);
            }
        }

        public ICommand CheckWebsiteAuthTokenCommand
        {
            get
            {
                if (_checkWebsiteAuthTokenCommand == null)
                {
                    _checkWebsiteAuthTokenCommand = new DelegateCommand<string>(CheckWebsiteAuthToken);
                }

                return _checkWebsiteAuthTokenCommand;
            }
        }

        public ICommand CheckRedirectUrlCommand
        {
            get
            {
                if (_checkRedirectUrlCommand == null)
                {
                    _checkRedirectUrlCommand = new DelegateCommand<Uri>(CheckRedirectUrl);
                }

                return _checkRedirectUrlCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new DelegateCommand(Cancel);
                }

                return _cancelCommand;
            }
        }

        #endregion Properties

        #region Methods

        public string GetLoginUrl()
        {
            if (_subOnly)
            {
                return "https://www.twitch.tv/login";
            }
            else
            {
                return $"https://id.twitch.tv/oauth2/authorize?client_id={ Constants.ClientId }&redirect_uri={ Constants.RedirectUrl }&response_type=token&scope=user:read:subscriptions&force_verify=true";
            }
        }

        private void CheckWebsiteAuthToken(string authToken)
        {
            if (Monitor.TryEnter(_checkAuthTokenLockObject))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(authToken))
                    {
                        return;
                    }

                    if (_authService.ValidateAuthentication(authToken, true))
                    {
                        FireSubOnlyAuthenticationSuccess();
                        _notificationService.ShowNotification("Sub-Only support successfully enabled!");
                    }
                    else
                    {
                        FireSubOnlyAuthenticationFailed();
                        _dialogService.ShowMessageBox("Access Token could not be verified! Authentication aborted!", "Error", MessageBoxButton.OK);
                    }
                }
                finally
                {
                    Monitor.Exit(_checkAuthTokenLockObject);
                }
            }
        }

        private void CheckRedirectUrl(Uri url)
        {
            try
            {
                lock (_commandLockObject)
                {
                    string urlStr = url?.OriginalString;

                    if (!SubOnly && !string.IsNullOrWhiteSpace(urlStr) && urlStr.StartsWith(Constants.RedirectUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        NameValueCollection urlParams = HttpUtility.ParseQueryString(url.Query);

                        int tokenIndex = urlStr.IndexOf("#access_token=");

                        if (tokenIndex >= 0)
                        {
                            tokenIndex += 14; // #access_token= -> 14 chars

                            int paramIndex = urlStr.IndexOf("&");

                            string accessToken = null;

                            if (paramIndex >= 0)
                            {
                                accessToken = urlStr.Substring(tokenIndex, paramIndex - tokenIndex);
                            }
                            else
                            {
                                accessToken = urlStr.Substring(tokenIndex);
                            }

                            if (string.IsNullOrWhiteSpace(accessToken))
                            {
                                _dialogService.ShowMessageBox("Twitch did not respond with an access token! Authentication aborted!", "Error", MessageBoxButton.OK);
                                FireAuthenticationFailed();
                            }
                            else
                            {
                                if (_authService.ValidateAuthentication(accessToken, false))
                                {
                                    FireAuthenticationSuccess();
                                    _notificationService.ShowNotification("Twitch authentication successful!");
                                }
                                else
                                {
                                    FireAuthenticationFailed();
                                    _dialogService.ShowMessageBox("Access Token could not be verified! Authentication aborted!", "Error", MessageBoxButton.OK);
                                }
                            }
                        }
                        else if (urlParams.ContainsKey("error"))
                        {
                            string error = urlParams.Get("error");

                            if (!string.IsNullOrWhiteSpace(error) && error.Equals("access_denied", StringComparison.OrdinalIgnoreCase))
                            {
                                FireAuthenticationFailed();
                                _notificationService.ShowNotification("Twitch authentication has been canceled!");
                            }
                            else
                            {
                                void UnspecifiedError()
                                {
                                    _dialogService.ShowMessageBox("Twitch responded with an unspecified error! Authentication aborted!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    FireAuthenticationFailed();
                                }

                                if (urlParams.ContainsKey("error_description"))
                                {
                                    string errorDesc = urlParams.Get("error_description");

                                    if (string.IsNullOrWhiteSpace(errorDesc))
                                    {
                                        UnspecifiedError();
                                    }
                                    else
                                    {
                                        _dialogService.ShowMessageBox(
                                            "Twitch responded with an error:" +
                                            Environment.NewLine + Environment.NewLine +
                                            "\"" + errorDesc + "\"" +
                                            Environment.NewLine + Environment.NewLine +
                                            "Authentication aborted!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        FireAuthenticationFailed();
                                    }
                                }
                                else
                                {
                                    UnspecifiedError();
                                }
                            }
                        }
                        else
                        {
                            _dialogService.ShowMessageBox("Twitch responded neither with an access token nor an error! Authentication aborted!", "Error", MessageBoxButton.OK);
                            FireAuthenticationFailed();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void FireAuthenticationSuccess()
        {
            _eventAggregator.GetEvent<AuthResultEvent>().Publish(true);
        }

        private void FireAuthenticationFailed()
        {
            _eventAggregator.GetEvent<AuthResultEvent>().Publish(false);
        }

        private void FireSubOnlyAuthenticationSuccess()
        {
            _eventAggregator.GetEvent<SubOnlyAuthResultEvent>().Publish(true);
        }

        private void FireSubOnlyAuthenticationFailed()
        {
            _eventAggregator.GetEvent<SubOnlyAuthResultEvent>().Publish(false);
        }

        private void Cancel()
        {
            try
            {
                lock (_commandLockObject)
                {
                    if (SubOnly)
                    {
                        FireSubOnlyAuthenticationFailed();
                    }
                    else
                    {
                        FireAuthenticationFailed();
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        protected override List<MenuCommand> BuildMenu()
        {
            List<MenuCommand> menuCommands = base.BuildMenu();

            if (menuCommands == null)
            {
                menuCommands = new List<MenuCommand>();
            }

            menuCommands.Add(new MenuCommand(CancelCommand, "Cancel", "Times"));

            return menuCommands;
        }

        #endregion Methods
    }
}