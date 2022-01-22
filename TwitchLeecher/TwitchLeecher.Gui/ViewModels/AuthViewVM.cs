using System;
using System.Windows.Input;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class AuthViewVM : ViewModelBase
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        private ICommand _connectCommand;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructor

        public AuthViewVM(
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;

            _commandLockObject = new object();
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
                    _navigationService.ShowLogin();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        #endregion Methods
    }
}