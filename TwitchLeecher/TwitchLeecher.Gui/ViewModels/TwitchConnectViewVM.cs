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

        private IDialogService dialogService;
        private ITwitchService twitchService;
        private INavigationService navigationService;
        private INotificationService notificationService;

        private ICommand navigatedCommand;
        private ICommand cancelCommand;

        private readonly object commandLockObject;

        #endregion Fields

        #region Constructor

        public TwitchConnectViewVM(
            IDialogService dialogService,
            ITwitchService twitchService,
            INavigationService navigationService,
            INotificationService notificationService)
        {
            this.dialogService = dialogService;
            this.twitchService = twitchService;
            this.navigationService = navigationService;
            this.notificationService = notificationService;

            this.commandLockObject = new object();
        }

        #endregion Constructor

        #region Properties

        public ICommand NavigatedCommand
        {
            get
            {
                if (this.navigatedCommand == null)
                {
                    this.navigatedCommand = new DelegateCommand<Uri>(this.Navigated);
                }

                return this.navigatedCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                {
                    this.cancelCommand = new DelegateCommand(this.Cancel);
                }

                return this.cancelCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Navigated(Uri url)
        {
            try
            {
                lock (this.commandLockObject)
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
                                this.dialogService.ShowMessageBox("Twitch did not respond with an access token! Authorization aborted!", "Error", MessageBoxButton.OK);
                                this.navigationService.NavigateBack();
                            }
                            else
                            {
                                if (this.twitchService.Authorize(accessToken))
                                {
                                    this.navigationService.ShowRevokeAuthorization();
                                    this.notificationService.ShowNotification("Twitch authorization successful!");
                                }
                                else
                                {
                                    this.dialogService.ShowMessageBox("Access Token '" + accessToken + "' could not be verified! Authorization aborted!", "Error", MessageBoxButton.OK);
                                    this.navigationService.NavigateBack();
                                }
                            }
                        }
                        else if (urlParams.ContainsKey("error"))
                        {
                            string error = urlParams.Get("error");

                            if (!string.IsNullOrWhiteSpace(error) && error.Equals("access_denied", StringComparison.OrdinalIgnoreCase))
                            {
                                this.navigationService.NavigateBack();
                                this.notificationService.ShowNotification("Twitch authorization has been canceled!");
                            }
                            else
                            {
                                Action unspecifiedError = () =>
                                {
                                    this.dialogService.ShowMessageBox("Twitch responded with an unspecified error! Authorization aborted!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    this.navigationService.NavigateBack();
                                };

                                if (urlParams.ContainsKey("error_description"))
                                {
                                    string errorDesc = urlParams.Get("error_description");

                                    if (string.IsNullOrWhiteSpace(errorDesc))
                                    {
                                        unspecifiedError();
                                    }
                                    else
                                    {
                                        this.dialogService.ShowMessageBox(
                                            "Twitch responded with an error:" +
                                            Environment.NewLine + Environment.NewLine +
                                            "\"" + errorDesc + "\"" +
                                            Environment.NewLine + Environment.NewLine +
                                            "Authorization aborted!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        this.navigationService.NavigateBack();
                                    }
                                }
                                else
                                {
                                    unspecifiedError();
                                }
                            }
                        }
                        else
                        {
                            this.dialogService.ShowMessageBox("Twitch responded neither with an access token nor an error! Authorization aborted!", "Error", MessageBoxButton.OK);
                            this.navigationService.NavigateBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void Cancel()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.navigationService.NavigateBack();
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        protected override List<MenuCommand> BuildMenu()
        {
            List<MenuCommand> menuCommands = base.BuildMenu();

            if (menuCommands == null)
            {
                menuCommands = new List<MenuCommand>();
            }

            menuCommands.Add(new MenuCommand(this.CancelCommand, "Cancel", "Times"));

            return menuCommands;
        }

        #endregion Methods
    }
}