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

        private IDialogService dialogService;
        private INavigationService navigationService;
        private IEventAggregator eventAggregator;

        private ICommand connectCommand;

        private readonly object commandLockObject;

        #endregion Fields

        #region Constructor

        public AuthorizeViewVM(
            IDialogService dialogService,
            INavigationService navigationService,
            IEventAggregator eventAggregator)
        {
            this.dialogService = dialogService;
            this.navigationService = navigationService;
            this.eventAggregator = eventAggregator;

            this.commandLockObject = new object();

            this.eventAggregator.GetEvent<IsAuthorizedChangedEvent>().Subscribe(this.IsAuthorizedChanged);
        }

        #endregion Constructor

        #region Properties

        public ICommand ConnectCommand
        {
            get
            {
                if (this.connectCommand == null)
                {
                    this.connectCommand = new DelegateCommand(this.Connect);
                }

                return this.connectCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Connect()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.navigationService.ShowTwitchConnect();
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void IsAuthorizedChanged(bool isAuthorized)
        {
            if (isAuthorized)
            {
                this.navigationService.ShowRevokeAuthorization();
            }
        }

        #endregion Methods
    }
}