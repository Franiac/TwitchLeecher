using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class LogViewVM : ViewModelBase
    {
        #region Fields

        private TwitchVideoDownload download;

        private ICommand copyCommand;
        private ICommand closeCommand;

        private IDialogService dialogService;
        private INavigationService navigationService;

        private readonly object commandLockObject;

        #endregion Fields

        #region Constructors

        public LogViewVM(
            IDialogService dialogService,
            INavigationService navigationService)
        {
            this.dialogService = dialogService;
            this.navigationService = navigationService;

            this.commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public TwitchVideoDownload Download
        {
            get
            {
                return this.download;
            }
            set
            {
                this.SetProperty(ref this.download, value, nameof(this.Download));
            }
        }

        public ICommand CopyCommand
        {
            get
            {
                if (this.copyCommand == null)
                {
                    this.copyCommand = new DelegateCommand(this.Copy);
                }

                return this.copyCommand;
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (this.closeCommand == null)
                {
                    this.closeCommand = new DelegateCommand(this.Close);
                }

                return this.closeCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Copy()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    Clipboard.SetText(this.download?.Log);
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void Close()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.navigationService.NavigateBack();
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

            menuCommands.Add(new MenuCommand(this.CopyCommand, "Copy", "Copy"));
            menuCommands.Add(new MenuCommand(this.CloseCommand, "Back", "ArrowLeft"));

            return menuCommands;
        }

        #endregion Methods
    }
}