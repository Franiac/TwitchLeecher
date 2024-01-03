using System;
using System.Collections.Generic;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.Views;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class LogViewVM : ViewModelBase
    {
        #region Fields

        private TwitchVideoDownload _download;

        private ICommand _copyCommand;
        private ICommand _closeCommand;

        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly MainWindow _window;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructors

        public LogViewVM(
            IDialogService dialogService,
            INavigationService navigationService,
            MainWindow window)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _window = window;

            _commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public TwitchVideoDownload Download
        {
            get { return _download; }
            set { SetProperty(ref _download, value, nameof(Download)); }
        }

        public ICommand CopyCommand
        {
            get
            {
                if (_copyCommand == null)
                {
                    _copyCommand = new DelegateCommand(Copy);
                }

                return _copyCommand;
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new DelegateCommand(Close);
                }

                return _closeCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Copy()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _window.Clipboard?.SetTextAsync(_download?.Log).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void Close()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _navigationService.NavigateBack();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        protected override List<MenuCommand> BuildMenu()
        {
            List<MenuCommand> menuCommands = base.BuildMenu();

            if (menuCommands == null)
            {
                menuCommands = new List<MenuCommand>();
            }

            menuCommands.Add(new MenuCommand(CopyCommand, "Copy", "Solid_Copy"));
            menuCommands.Add(new MenuCommand(CloseCommand, "Back", "Solid_ArrowLeft"));

            return menuCommands;
        }

        #endregion Methods
    }
}