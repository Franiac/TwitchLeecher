using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class PreferencesViewVM : ViewModelBase
    {
        #region Fields

        private IDialogService dialogService;
        private INotificationService notificationService;
        private IPreferencesService preferencesService;

        private Preferences currentPreferences;
        private IEnumerable<VideoQuality> videoQualityList;

        private ICommand chooseDownloadTempFolderCommand;
        private ICommand chooseDownloadFolderCommand;
        private ICommand saveCommand;
        private ICommand undoCommand;
        private ICommand defaultsCommand;

        private readonly object commandLockObject;

        #endregion Fields

        #region Constructors

        public PreferencesViewVM(
            IDialogService dialogService,
            INotificationService notificationService,
            IPreferencesService preferencesService)
        {
            this.dialogService = dialogService;
            this.notificationService = notificationService;
            this.preferencesService = preferencesService;

            this.commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public Preferences CurrentPreferences
        {
            get
            {
                if (this.currentPreferences == null)
                {
                    this.currentPreferences = this.preferencesService.CurrentPreferences.Clone();
                }

                return this.currentPreferences;
            }

            private set
            {
                this.SetProperty(ref this.currentPreferences, value);
            }
        }

        public IEnumerable<VideoQuality> VideoQualityList
        {
            get
            {
                if (this.videoQualityList == null)
                {
                    this.videoQualityList = Enum.GetValues(typeof(VideoQuality)).Cast<VideoQuality>();
                }

                return this.videoQualityList;
            }
        }

        public ICommand ChooseDownloadTempFolderCommand
        {
            get
            {
                if (this.chooseDownloadTempFolderCommand == null)
                {
                    this.chooseDownloadTempFolderCommand = new DelegateCommand(this.ChooseDownloadTempFolder);
                }

                return this.chooseDownloadTempFolderCommand;
            }
        }

        public ICommand ChooseDownloadFolderCommand
        {
            get
            {
                if (this.chooseDownloadFolderCommand == null)
                {
                    this.chooseDownloadFolderCommand = new DelegateCommand(this.ChooseDownloadFolder);
                }

                return this.chooseDownloadFolderCommand;
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (this.saveCommand == null)
                {
                    this.saveCommand = new DelegateCommand(this.Save);
                }

                return this.saveCommand;
            }
        }

        public ICommand UndoCommand
        {
            get
            {
                if (this.undoCommand == null)
                {
                    this.undoCommand = new DelegateCommand(this.Undo);
                }

                return this.undoCommand;
            }
        }

        public ICommand DefaultsCommand
        {
            get
            {
                if (this.defaultsCommand == null)
                {
                    this.defaultsCommand = new DelegateCommand(this.Defaults);
                }

                return this.defaultsCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void ChooseDownloadTempFolder()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.dialogService.ShowFolderBrowserDialog(this.CurrentPreferences.DownloadTempFolder, this.ChooseDownloadTempFolderCallback);
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseDownloadTempFolderCallback(bool cancelled, string folder)
        {
            try
            {
                if (!cancelled)
                {
                    this.CurrentPreferences.DownloadTempFolder = folder;
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseDownloadFolder()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.dialogService.ShowFolderBrowserDialog(this.CurrentPreferences.DownloadFolder, this.ChooseDownloadFolderCallback);
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseDownloadFolderCallback(bool cancelled, string folder)
        {
            try
            {
                if (!cancelled)
                {
                    this.CurrentPreferences.DownloadFolder = folder;
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void Save()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.dialogService.SetBusy();
                    this.Validate();

                    if (!this.HasErrors)
                    {
                        this.preferencesService.Save(this.currentPreferences);
                        this.CurrentPreferences = null;
                        this.notificationService.ShowNotification("Preferences saved");
                    }
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void Undo()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    MessageBoxResult result = this.dialogService.ShowMessageBox("Undo current changes and reload last saved preferences?", "Undo", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        this.dialogService.SetBusy();
                        this.CurrentPreferences = null;
                    }
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void Defaults()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    MessageBoxResult result = this.dialogService.ShowMessageBox("Load default preferences?", "Defaults", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        this.dialogService.SetBusy();
                        this.preferencesService.Save(this.preferencesService.CreateDefault());
                        this.CurrentPreferences = null;
                    }
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

            string currentProperty = nameof(this.CurrentPreferences);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                this.CurrentPreferences?.Validate();

                if (this.CurrentPreferences.HasErrors)
                {
                    this.AddError(currentProperty, "Invalid Preferences!");
                }
            }
        }

        public override void OnBeforeHidden()
        {
            try
            {
                this.CurrentPreferences = null;
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

            menuCommands.Add(new MenuCommand(this.SaveCommand, "Save", "Save"));
            menuCommands.Add(new MenuCommand(this.UndoCommand, "Undo", "Undo"));
            menuCommands.Add(new MenuCommand(this.DefaultsCommand, "Default", "Wrench"));

            return menuCommands;
        }

        #endregion Methods
    }
}