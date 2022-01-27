using System;
using System.Windows.Input;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class SubOnlyRevokeAuthViewVM : ViewModelBase
    {
        #region Fields

        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        private ICommand _disableSubOnlyCommand;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructor

        public SubOnlyRevokeAuthViewVM(
            IAuthService authService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _authService = authService;
            _dialogService = dialogService;
            _navigationService = navigationService;

            _commandLockObject = new object();
        }

        #endregion Constructor

        #region Properties

        public ICommand DisableSubOnlyCommand
        {
            get
            {
                if (_disableSubOnlyCommand == null)
                {
                    _disableSubOnlyCommand = new DelegateCommand(DisableSubOnly);
                }

                return _disableSubOnlyCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void DisableSubOnly()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _authService.RevokeAuthenticationSubOnly();
                    _navigationService.ShowSubOnlyAuth();
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