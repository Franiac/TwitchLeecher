using System;
using System.Windows.Input;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.ViewModels
{
    public class AuthorizeViewVM : ViewModelBase
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IEventAggregator _eventAggregator;

        private ICommand _connectCommand;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructor

        public AuthorizeViewVM(
            IDialogService dialogService,
            INavigationService navigationService,
            IEventAggregator eventAggregator)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _eventAggregator = eventAggregator;

            _commandLockObject = new object();

            _eventAggregator.GetEvent<IsAuthorizedChangedEvent>().Subscribe(IsAuthorizedChanged);
        }

        #endregion Constructor

        #region Properties

        public ICommand ConnectCommand
        {
            get
            {
                if (_connectCommand == null)
                {
                    _connectCommand = new DelegateCommand(Connect);
                }

                return _connectCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Connect()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _navigationService.ShowTwitchConnect();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void IsAuthorizedChanged(bool isAuthorized)
        {
            if (isAuthorized)
            {
                _navigationService.ShowRevokeAuthorization();
            }
        }

        #endregion Methods
    }
}