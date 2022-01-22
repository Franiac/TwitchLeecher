using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core;
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

        private ICommand _navigatingCommand;
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
        }

        #endregion Constructor

        #region Properties

        public ICommand NavigatingCommand
        {
            get
            {
                if (_navigatingCommand == null)
                {
                    _navigatingCommand = new DelegateCommand<Uri>(Navigating);
                }

                return _navigatingCommand;
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
            return $"https://id.twitch.tv/oauth2/authorize?client_id={ Constants.ClientId }&redirect_uri={ Constants.RedirectUrl }&response_type=token&scope=user:read:subscriptions&force_verify=true";
        }

        private void Navigating(Uri url)
        {
            try
            {
                lock (_commandLockObject)
                {
                    string urlStr = url?.OriginalString;

                    if (string.IsNullOrWhiteSpace(urlStr) || !urlStr.StartsWith(Constants.RedirectUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

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
                            FireFailed();
                        }
                        else
                        {
                            if (_authService.ValidateAuthentication(accessToken))
                            {
                                FireSuccess();
                                _notificationService.ShowNotification("Twitch authentication successful!");
                            }
                            else
                            {
                                _dialogService.ShowMessageBox("Access Token '" + accessToken + "' could not be verified! Authentication aborted!", "Error", MessageBoxButton.OK);
                                FireFailed();
                            }
                        }
                    }
                    else if (urlParams.ContainsKey("error"))
                    {
                        string error = urlParams.Get("error");

                        if (!string.IsNullOrWhiteSpace(error) && error.Equals("access_denied", StringComparison.OrdinalIgnoreCase))
                        {
                            FireFailed();
                            _notificationService.ShowNotification("Twitch authentication has been canceled!");
                        }
                        else
                        {
                            void UnspecifiedError()
                            {
                                _dialogService.ShowMessageBox("Twitch responded with an unspecified error! Authentication aborted!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                FireFailed();
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
                                    FireFailed();
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
                        FireFailed();
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void FireSuccess()
        {
            _eventAggregator.GetEvent<AuthenticationResultEvent>().Publish(true);
        }

        private void FireFailed()
        {
            _eventAggregator.GetEvent<AuthenticationResultEvent>().Publish(false);
        }

        private void Cancel()
        {
            try
            {
                lock (_commandLockObject)
                {
                    FireFailed();
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