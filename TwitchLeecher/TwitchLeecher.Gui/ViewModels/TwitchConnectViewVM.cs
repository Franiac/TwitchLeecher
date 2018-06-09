using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Extensions;

namespace TwitchLeecher.Gui.ViewModels
{
    public class TwitchConnectViewVM : ViewModelBase
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly ITwitchService _twitchService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;

        private ICommand _navigatingCommand;
        private ICommand _cancelCommand;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructor

        public TwitchConnectViewVM(
            IDialogService dialogService,
            ITwitchService twitchService,
            INavigationService navigationService,
            INotificationService notificationService)
        {
            _dialogService = dialogService;
            _twitchService = twitchService;
            _navigationService = navigationService;
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

        private void Navigating(Uri url)
        {
            try
            {
                lock (_commandLockObject)
                {
                    string urlStr = url?.OriginalString;

                    if (!string.IsNullOrWhiteSpace(urlStr) && urlStr.StartsWith("http://www.tl.com", StringComparison.OrdinalIgnoreCase))
                    {
                        NameValueCollection urlParams = HttpUtility.ParseQueryString(url.Query);

                        int tokenIndex = urlStr.IndexOf("#access_token=");

                        if (tokenIndex >= 0)
                        {
                            tokenIndex = tokenIndex + 14; // #access_token= -> 14 chars

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
                                _dialogService.ShowMessageBox("Twitch did not respond with an access token! Authorization aborted!", "Error", MessageBoxButton.OK);
                                _navigationService.NavigateBack();
                            }
                            else
                            {
                                if (_twitchService.Authorize(accessToken))
                                {
                                    _navigationService.ShowRevokeAuthorization();
                                    _notificationService.ShowNotification("Twitch authorization successful!");
                                }
                                else
                                {
                                    _dialogService.ShowMessageBox("Access Token '" + accessToken + "' could not be verified! Authorization aborted!", "Error", MessageBoxButton.OK);
                                    _navigationService.NavigateBack();
                                }
                            }
                        }
                        else if (urlParams.ContainsKey("error"))
                        {
                            string error = urlParams.Get("error");

                            if (!string.IsNullOrWhiteSpace(error) && error.Equals("access_denied", StringComparison.OrdinalIgnoreCase))
                            {
                                _navigationService.NavigateBack();
                                _notificationService.ShowNotification("Twitch authorization has been canceled!");
                            }
                            else
                            {
                                void UnspecifiedError()
                                {
                                    _dialogService.ShowMessageBox("Twitch responded with an unspecified error! Authorization aborted!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    _navigationService.NavigateBack();
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
                                            "Authorization aborted!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        _navigationService.NavigateBack();
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
                            _dialogService.ShowMessageBox("Twitch responded neither with an access token nor an error! Authorization aborted!", "Error", MessageBoxButton.OK);
                            _navigationService.NavigateBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void Cancel()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _navigationService.NavigateBack();
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