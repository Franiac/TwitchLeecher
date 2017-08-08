using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class PreferencesViewVM : ViewModelBase
    {
        #region Fields

        private IDialogService _dialogService;
        private INotificationService _notificationService;
        private IPreferencesService _preferencesService;

        private Preferences _currentPreferences;

        private ICommand _chooseDownloadTempFolderCommand;
        private ICommand _chooseDownloadFolderCommand;
        private ICommand _saveCommand;
        private ICommand _undoCommand;
        private ICommand _defaultsCommand;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructors

        public PreferencesViewVM(
            IDialogService dialogService,
            INotificationService notificationService,
            IPreferencesService preferencesService)
        {
            _dialogService = dialogService;
            _notificationService = notificationService;
            _preferencesService = preferencesService;

            _commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public Preferences CurrentPreferences
        {
            get
            {
                if (_currentPreferences == null)
                {
                    _currentPreferences = _preferencesService.CurrentPreferences.Clone();
                }

                return _currentPreferences;
            }

            private set
            {
                SetProperty(ref _currentPreferences, value);
            }
        }

        public ICommand ChooseDownloadTempFolderCommand
        {
            get
            {
                if (_chooseDownloadTempFolderCommand == null)
                {
                    _chooseDownloadTempFolderCommand = new DelegateCommand(ChooseDownloadTempFolder);
                }

                return _chooseDownloadTempFolderCommand;
            }
        }

        public ICommand ChooseDownloadFolderCommand
        {
            get
            {
                if (_chooseDownloadFolderCommand == null)
                {
                    _chooseDownloadFolderCommand = new DelegateCommand(ChooseDownloadFolder);
                }

                return _chooseDownloadFolderCommand;
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new DelegateCommand(Save);
                }

                return _saveCommand;
            }
        }

        public ICommand UndoCommand
        {
            get
            {
                if (_undoCommand == null)
                {
                    _undoCommand = new DelegateCommand(Undo);
                }

                return _undoCommand;
            }
        }

        public ICommand DefaultsCommand
        {
            get
            {
                if (_defaultsCommand == null)
                {
                    _defaultsCommand = new DelegateCommand(Defaults);
                }

                return _defaultsCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void ChooseDownloadTempFolder()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _dialogService.ShowFolderBrowserDialog(CurrentPreferences.DownloadTempFolder, ChooseDownloadTempFolderCallback);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseDownloadTempFolderCallback(bool cancelled, string folder)
        {
            try
            {
                if (!cancelled)
                {
                    CurrentPreferences.DownloadTempFolder = folder;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseDownloadFolder()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _dialogService.ShowFolderBrowserDialog(CurrentPreferences.DownloadFolder, ChooseDownloadFolderCallback);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseDownloadFolderCallback(bool cancelled, string folder)
        {
            try
            {
                if (!cancelled)
                {
                    CurrentPreferences.DownloadFolder = folder;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void Save()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _dialogService.SetBusy();
                    Validate();

                    if (!HasErrors)
                    {
                        _preferencesService.Save(_currentPreferences);
                        CurrentPreferences = null;
                        _notificationService.ShowNotification("Preferences saved");
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void Undo()
        {
            try
            {
                lock (_commandLockObject)
                {
                    MessageBoxResult result = _dialogService.ShowMessageBox("Undo current changes and reload last saved preferences?", "Undo", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        _dialogService.SetBusy();
                        CurrentPreferences = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void Defaults()
        {
            try
            {
                lock (_commandLockObject)
                {
                    MessageBoxResult result = _dialogService.ShowMessageBox("Load default preferences?", "Defaults", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        _dialogService.SetBusy();
                        _preferencesService.Save(_preferencesService.CreateDefault());
                        CurrentPreferences = null;
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(CurrentPreferences);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                CurrentPreferences?.Validate();

                if (CurrentPreferences.HasErrors)
                {
                    AddError(currentProperty, "Invalid Preferences!");
                }
            }
        }

        public override void OnBeforeHidden()
        {
            try
            {
                CurrentPreferences = null;
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

            menuCommands.Add(new MenuCommand(SaveCommand, "Save", "Save"));
            menuCommands.Add(new MenuCommand(UndoCommand, "Undo", "Undo"));
            menuCommands.Add(new MenuCommand(DefaultsCommand, "Default", "Wrench"));

            return menuCommands;
        }

        #endregion Methods
    }
}