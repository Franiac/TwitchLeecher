using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class UpdateInfoWindowVM : ViewModelBase
    {
        #region Fields

        private UpdateInfo updateInfo;
        private IDialogService dialogService;
        private ICommand downloadCommand;
        private ICommand closeCommand;

        #endregion Fields

        #region Constructor

        public UpdateInfoWindowVM(IDialogService dialogService)
        {
            this.dialogService = dialogService;
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

        public ICommand CloseCommand
        {
            get
            {
                if (this.closeCommand == null)
                {
                    this.closeCommand = new DelegateCommand<Window>(this.Close);
                }

                return this.closeCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Download()
        {
            try
            {
                Process.Start(this.updateInfo.DownloadUrl);
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void Close(Window window)
        {
            try
            {
                window.DialogResult = false;
                window.Close();
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        #endregion Methods
    }
}