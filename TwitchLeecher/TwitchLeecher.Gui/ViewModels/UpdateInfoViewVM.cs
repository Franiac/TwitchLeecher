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

        private UpdateInfo updateInfo;

        private IDialogService dialogService;

        private ICommand downloadCommand;

        private readonly object commandLockObject;

        #endregion Fields

        #region Constructor

        public UpdateInfoViewVM(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            this.commandLockObject = new object();
        }

        #endregion Constructor

        #region Properties

        public UpdateInfo UpdateInfo
        {
            get
            {
                return this.updateInfo;
            }
            set
            {
                this.updateInfo = value;
            }
        }

        public ICommand DownloadCommand
        {
            get
            {
                if (this.downloadCommand == null)
                {
                    this.downloadCommand = new DelegateCommand(this.Download);
                }

                return this.downloadCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Download()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    Process.Start(this.updateInfo.DownloadUrl);
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        protected override List<MenuCommand> BuildMenu()
        {
            List<MenuCommand> menuCommands = base.BuildMenu();

            if (menuCommands == null)
            {
                menuCommands = new List<MenuCommand>();
            }

            menuCommands.Add(new MenuCommand(this.DownloadCommand, "Download", "Download"));

            return menuCommands;
        }

        #endregion Methods
    }
}