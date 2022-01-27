using System;
using System.Windows.Input;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class SubOnlyAuthViewVM : ViewModelBase
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        private ICommand _enableSubOnlyCommand;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructor

        public SubOnlyAuthViewVM(
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;

            _commandLockObject = new object();
        }

        #endregion Constructor

        #region Properties

        public ICommand EnableSubOnlyCommand
        {
            get
            {
                if (_enableSubOnlyCommand == null)
                {
                    _enableSubOnlyCommand = new DelegateCommand(EnableSubOnly);
                }

                return _enableSubOnlyCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void EnableSubOnly()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _navigationService.ShowLogin(true);
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