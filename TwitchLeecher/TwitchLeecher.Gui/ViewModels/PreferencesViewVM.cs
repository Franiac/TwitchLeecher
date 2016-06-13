using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Services;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class PreferencesViewVM : ViewModelBase
    {
        #region Fields

        private IGuiService guiService;
        private IPreferencesService preferencesService;

        private Preferences currentPreferences;
        private IEnumerable<VideoQuality> videoQualityList;

        private ICommand chooseDownloadTempFolderCommand;
        private ICommand chooseDownloadFolderCommand;
        private ICommand saveCommand;
        private ICommand undoCommand;
        private ICommand defaultsCommand;

        #endregion Fields

        #region Constructors

        public PreferencesViewVM(IGuiService guiService, IPreferencesService preferencesService)
        {
            this.guiService = guiService;
            this.preferencesService = preferencesService;
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
                this.guiService.ShowFolderBrowserDialog(this.CurrentPreferences.DownloadTempFolder, this.ChooseDownloadTempFolderCallback);
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
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
                this.guiService.ShowAndLogException(ex);
            }
        }

        private void ChooseDownloadFolder()
        {
            try
            {
                this.guiService.ShowFolderBrowserDialog(this.CurrentPreferences.DownloadFolder, this.ChooseDownloadFolderCallback);
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
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
                this.guiService.ShowAndLogException(ex);
            }
        }

        private void Save()
        {
            try
            {
                this.guiService.SetBusy();
                this.Validate();

                if (!this.HasErrors)
                {
                    this.preferencesService.Save(this.currentPreferences);
                    this.CurrentPreferences = null;
                    this.guiService.ShowNotification("Preferences saved");
                }
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        private void Undo()
        {
            try
            {
                MessageBoxResult result = this.guiService.ShowMessageBox("Undo current changes and reload last saved preferences?", "Undo", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    this.guiService.SetBusy();
                    this.CurrentPreferences = null;
                }
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        private void Defaults()
        {
            try
            {
                MessageBoxResult result = this.guiService.ShowMessageBox("Load default preferences?", "Defaults", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    this.guiService.SetBusy();
                    this.preferencesService.Save(this.preferencesService.CreateDefault());
                    this.CurrentPreferences = null;
                }
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
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
                this.guiService.ShowAndLogException(ex);
            }
        }

        #endregion Methods
    }
}