using System;
using System.Diagnostics;
using System.Windows.Input;
using TwitchLeecher.Core.Constants;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class AuthViewVM : ViewModelBase
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IAuthListener _authListener;

        private ICommand _connectCommand;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructor

        public AuthViewVM(
            IDialogService dialogService,
            INavigationService navigationService,
            IAuthListener authListener)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _authListener = authListener;

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
                    _ = _authListener.StartListenForToken();
                    var psi = new ProcessStartInfo($"https://id.twitch.tv/oauth2/authorize?client_id={Constants.ClientId}&redirect_uri={Constants.RedirectUrl}&response_type=token&scope=user:read:subscriptions user_subscriptions&force_verify=true")
                    {
                        UseShellExecute = true,
                        Verb = "open"
                    };
                    Process.Start(psi);
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