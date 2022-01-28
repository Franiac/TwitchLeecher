using System;
using System.Windows;
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
        private readonly IDownloadService _downloadService;
        private readonly INavigationService _navigationService;

        private ICommand _disableSubOnlyCommand;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructor

        public SubOnlyRevokeAuthViewVM(
            IAuthService authService,
            IDialogService dialogService,
            IDownloadService downloadService,
            INavigationService navigationService)
        {
            _authService = authService;
            _dialogService = dialogService;
            _downloadService = downloadService;
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
                    _downloadService.Pause();

                    if (!_downloadService.CanShutdown())
                    {
                        MessageBoxResult result = _dialogService.ShowMessageBox("Do you want to abort all running downloads and disable sub-only support?", "Active Downloads", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.No)
                        {
                            _downloadService.Resume();
                            return;
                        }
                    }

                    _downloadService.Shutdown();

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