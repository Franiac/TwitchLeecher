using System;
using System.Windows.Input;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.ViewModels
{
    public class RevokeAuthorizationViewVM : ViewModelBase
    {
        #region Fields

        private IDialogService dialogService;
        private ITwitchService twitchService;
        private INavigationService navigationService;
        private INotificationService notificationService;
        private IEventAggregator eventAggregator;

        private ICommand revokeCommand;

        private readonly object commandLockObject;

        #endregion Fields

        #region Constructor

        public RevokeAuthorizationViewVM(
            IDialogService dialogService,
            ITwitchService twitchService,
            INavigationService navigationService,
            INotificationService notificationService,
            IEventAggregator eventAggregator)
        {
            this.dialogService = dialogService;
            this.twitchService = twitchService;
            this.navigationService = navigationService;
            this.notificationService = notificationService;
            this.eventAggregator = eventAggregator;

            this.commandLockObject = new object();

            this.eventAggregator.GetEvent<IsAuthorizedChangedEvent>().Subscribe(this.IsAuthorizedChanged);
        }

        #endregion Constructor

        #region Properties

        public ICommand RevokeCommand
        {
            get
            {
                if (this.revokeCommand == null)
                {
                    this.revokeCommand = new DelegateCommand(this.Revoke);
                }

                return this.revokeCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Revoke()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.twitchService.RevokeAuthorization();
                    this.navigationService.ShowAuthorize();
                    this.notificationService.ShowNotification("Twitch authorization has been revoked!");
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void IsAuthorizedChanged(bool isAuthorized)
        {
            if (!isAuthorized)
            {
                this.navigationService.ShowAuthorize();
            }
        }

        #endregion Methods
    }
}