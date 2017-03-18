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

        private IDialogService _dialogService;
        private ITwitchService _twitchService;
        private INavigationService _navigationService;
        private INotificationService _notificationService;
        private IEventAggregator _eventAggregator;

        private ICommand _revokeCommand;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructor

        public RevokeAuthorizationViewVM(
            IDialogService dialogService,
            ITwitchService twitchService,
            INavigationService navigationService,
            INotificationService notificationService,
            IEventAggregator eventAggregator)
        {
            _dialogService = dialogService;
            _twitchService = twitchService;
            _navigationService = navigationService;
            _notificationService = notificationService;
            _eventAggregator = eventAggregator;

            _commandLockObject = new object();

            _eventAggregator.GetEvent<IsAuthorizedChangedEvent>().Subscribe(IsAuthorizedChanged);
        }

        #endregion Constructor

        #region Properties

        public ICommand RevokeCommand
        {
            get
            {
                if (_revokeCommand == null)
                {
                    _revokeCommand = new DelegateCommand(Revoke);
                }

                return _revokeCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Revoke()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _twitchService.RevokeAuthorization();
                    _navigationService.ShowAuthorize();
                    _notificationService.ShowNotification("Twitch authorization has been revoked!");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void IsAuthorizedChanged(bool isAuthorized)
        {
            if (!isAuthorized)
            {
                _navigationService.ShowAuthorize();
            }
        }

        #endregion Methods
    }
}