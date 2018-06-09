using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class UpdateInfoViewVM : ViewModelBase
    {
        #region Fields

        private readonly IDialogService _dialogService;

        private readonly object _commandLockObject;

        private ICommand _downloadCommand;

        #endregion Fields

        #region Constructor

        public UpdateInfoViewVM(IDialogService dialogService)
        {
            _dialogService = dialogService;
            _commandLockObject = new object();
        }

        #endregion Constructor

        #region Properties

        public UpdateInfo UpdateInfo { get; set; }

        public ICommand DownloadCommand
        {
            get
            {
                if (_downloadCommand == null)
                {
                    _downloadCommand = new DelegateCommand(Download);
                }

                return _downloadCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Download()
        {
            try
            {
                lock (_commandLockObject)
                {
                    Process.Start(UpdateInfo.DownloadUrl);
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

            menuCommands.Add(new MenuCommand(DownloadCommand, "Download", "Download"));

            return menuCommands;
        }

        #endregion Methods
    }
}