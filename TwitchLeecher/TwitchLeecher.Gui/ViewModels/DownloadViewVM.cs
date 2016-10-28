using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class DownloadViewVM : ViewModelBase
    {
        #region Fields

        private DownloadParameters downloadParams;

        private ICommand chooseCommand;
        private ICommand downloadCommand;
        private ICommand cancelCommand;

        private IDialogService dialogService;
        private ITwitchService twitchService;
        private INavigationService navigationService;
        private INotificationService notificationService;

        private readonly object commandLockObject;

        #endregion Fields

        #region Constructors

        public DownloadViewVM(
            IDialogService dialogService,
            ITwitchService twitchService,
            INavigationService navigationService,
            INotificationService notificationService)
        {
            this.dialogService = dialogService;
            this.twitchService = twitchService;
            this.navigationService = navigationService;
            this.notificationService = notificationService;

            this.commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public DownloadParameters DownloadParams
        {
            get
            {
                return this.downloadParams;
            }
            set
            {
                this.SetProperty(ref this.downloadParams, value, nameof(this.DownloadParams));
            }
        }

        public string CropStartTime
        {
            get
            {
                return this.downloadParams.CropStartTime.ToString();
            }
            set
            {
                TimeSpan ts;

                if (TimeSpan.TryParse(value, out ts))
                {
                    this.downloadParams.CropStartTime = ts;
                }

                this.FirePropertyChanged(nameof(this.CropStartTime));
            }
        }

        public string CropEndTime
        {
            get
            {
                return this.downloadParams.CropEndTime.ToString();
            }
            set
            {
                TimeSpan ts;

                if (TimeSpan.TryParse(value, out ts))
                {
                    this.downloadParams.CropEndTime = ts;
                }

                this.FirePropertyChanged(nameof(this.CropEndTime));
            }
        }

        public ICommand ChooseCommand
        {
            get
            {
                if (this.chooseCommand == null)
                {
                    this.chooseCommand = new DelegateCommand(this.Choose);
                }

                return this.chooseCommand;
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

        public ICommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                {
                    this.cancelCommand = new DelegateCommand(this.Cancel);
                }

                return this.cancelCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Choose()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.dialogService.ShowFolderBrowserDialog(this.downloadParams.Folder, this.ChooseCallback);
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseCallback(bool cancelled, string folder)
        {
            try
            {
                if (!cancelled)
                {
                    this.downloadParams.Folder = folder;
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void Download()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.Validate();

                    if (!this.HasErrors)
                    {
                        if (File.Exists(this.downloadParams.FullPath))
                        {
                            MessageBoxResult result = this.dialogService.ShowMessageBox("The file already exists. Do you want to overwrite it?", "Download", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (result != MessageBoxResult.Yes)
                            {
                                return;
                            }
                        }

                        this.twitchService.Enqueue(downloadParams);
                        this.navigationService.NavigateBack();
                        this.notificationService.ShowNotification("Download added");
                    }
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void Cancel()
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

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(this.DownloadParams);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                this.DownloadParams?.Validate();

                if (this.twitchService.IsFileNameUsed(this.downloadParams.FullPath))
                {
                    this.DownloadParams.AddError(nameof(this.DownloadParams.Filename), "Another video is already being downloaded to this file!");
                }

                if (this.DownloadParams.HasErrors)
                {
                    this.AddError(currentProperty, "Invalid Download Parameters!");
                }
            }

            currentProperty = nameof(this.CropStartTime);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                this.DownloadParams?.Validate(currentProperty);

                if (this.DownloadParams.HasErrors)
                {
                    List<string> errors = this.DownloadParams.GetErrors(currentProperty) as List<string>;

                    if (errors != null && errors.Count > 0)
                    {
                        this.AddError(currentProperty, errors.First());
                    }
                }
            }

            currentProperty = nameof(this.CropEndTime);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                this.DownloadParams?.Validate(currentProperty);

                if (this.DownloadParams.HasErrors)
                {
                    List<string> errors = this.DownloadParams.GetErrors(currentProperty) as List<string>;

                    if (errors != null && errors.Count > 0)
                    {
                        this.AddError(currentProperty, errors.First());
                    }
                }
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
            menuCommands.Add(new MenuCommand(this.CancelCommand, "Cancel", "Times"));

            return menuCommands;
        }

        #endregion Methods
    }
}