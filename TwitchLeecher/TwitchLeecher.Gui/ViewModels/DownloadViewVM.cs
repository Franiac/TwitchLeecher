using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Extensions;

namespace TwitchLeecher.Gui.ViewModels
{
    public class DownloadViewVM : ViewModelBase
    {
        #region Fields

        private DownloadParameters _downloadParams;
        private bool _useCustomFilename;

        private ICommand _chooseCommand;
        private ICommand _downloadCommand;
        private ICommand _cancelCommand;

        private readonly IDialogService _dialogService;
        private readonly IFilenameService _filenameService;
        private readonly IPreferencesService _preferencesService;
        private readonly ITwitchService _twitchService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructors

        public DownloadViewVM(
            IDialogService dialogService,
            IFilenameService filenameService,
            IPreferencesService preferencesService,
            ITwitchService twitchService,
            INavigationService navigationService,
            INotificationService notificationService)
        {
            _dialogService = dialogService;
            _filenameService = filenameService;
            _preferencesService = preferencesService;
            _twitchService = twitchService;
            _navigationService = navigationService;
            _notificationService = notificationService;

            _commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public DownloadParameters DownloadParams
        {
            get
            {
                return _downloadParams;
            }
            set
            {
                if (_downloadParams != null)
                {
                    _downloadParams.PropertyChanged -= _downloadParams_PropertyChanged;
                }

                SetProperty(ref _downloadParams, value, nameof(DownloadParams));

                _downloadParams.PropertyChanged += _downloadParams_PropertyChanged;
            }
        }

        public bool UseCustomFilename
        {
            get
            {
                return _useCustomFilename;
            }
            set
            {
                SetProperty(ref _useCustomFilename, value, nameof(UseCustomFilename));

                if (!value)
                {
                    UpdateFilenameFromTemplate();
                }
            }
        }

        public int CropStartHours
        {
            get
            {
                return _downloadParams.CropStartTime.GetDaysInHours();
            }
            set
            {
                TimeSpan current = _downloadParams.CropStartTime;
                _downloadParams.CropStartTime = new TimeSpan(value, current.Minutes, current.Seconds);

                FirePropertyChanged(nameof(CropStartHours));
                FirePropertyChanged(nameof(CropStartMinutes));
                FirePropertyChanged(nameof(CropStartSeconds));
            }
        }

        public int CropStartMinutes
        {
            get
            {
                return _downloadParams.CropStartTime.Minutes;
            }
            set
            {
                TimeSpan current = _downloadParams.CropStartTime;
                _downloadParams.CropStartTime = new TimeSpan(current.GetDaysInHours(), value, current.Seconds);

                FirePropertyChanged(nameof(CropStartHours));
                FirePropertyChanged(nameof(CropStartMinutes));
                FirePropertyChanged(nameof(CropStartSeconds));
            }
        }

        public int CropStartSeconds
        {
            get
            {
                return _downloadParams.CropStartTime.Seconds;
            }
            set
            {
                TimeSpan current = _downloadParams.CropStartTime;
                _downloadParams.CropStartTime = new TimeSpan(current.GetDaysInHours(), current.Minutes, value);

                FirePropertyChanged(nameof(CropStartHours));
                FirePropertyChanged(nameof(CropStartMinutes));
                FirePropertyChanged(nameof(CropStartSeconds));
            }
        }

        public int CropEndHours
        {
            get
            {
                return _downloadParams.CropEndTime.GetDaysInHours();
            }
            set
            {
                TimeSpan current = _downloadParams.CropEndTime;
                _downloadParams.CropEndTime = new TimeSpan(value, current.Minutes, current.Seconds);

                FirePropertyChanged(nameof(CropEndHours));
                FirePropertyChanged(nameof(CropEndMinutes));
                FirePropertyChanged(nameof(CropEndSeconds));
            }
        }

        public int CropEndMinutes
        {
            get
            {
                return _downloadParams.CropEndTime.Minutes;
            }
            set
            {
                TimeSpan current = _downloadParams.CropEndTime;
                _downloadParams.CropEndTime = new TimeSpan(current.GetDaysInHours(), value, current.Seconds);

                FirePropertyChanged(nameof(CropEndHours));
                FirePropertyChanged(nameof(CropEndMinutes));
                FirePropertyChanged(nameof(CropEndSeconds));
            }
        }

        public int CropEndSeconds
        {
            get
            {
                return _downloadParams.CropEndTime.Seconds;
            }
            set
            {
                TimeSpan current = _downloadParams.CropEndTime;
                _downloadParams.CropEndTime = new TimeSpan(current.GetDaysInHours(), current.Minutes, value);

                FirePropertyChanged(nameof(CropEndHours));
                FirePropertyChanged(nameof(CropEndMinutes));
                FirePropertyChanged(nameof(CropEndSeconds));
            }
        }

        public ICommand ChooseCommand
        {
            get
            {
                if (_chooseCommand == null)
                {
                    _chooseCommand = new DelegateCommand(Choose);
                }

                return _chooseCommand;
            }
        }

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

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new DelegateCommand(Cancel);
                }

                return _cancelCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Choose()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _dialogService.ShowFolderBrowserDialog(_downloadParams.Folder, ChooseCallback);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ChooseCallback(bool cancelled, string folder)
        {
            try
            {
                if (!cancelled)
                {
                    _downloadParams.Folder = folder;
                    _downloadParams.Validate(nameof(DownloadParameters.Folder));
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void UpdateFilenameFromTemplate()
        {
            Preferences currentPrefs = _preferencesService.CurrentPreferences.Clone();

            TimeSpan? cropStartTime = _downloadParams.CropStart ? _downloadParams.CropStartTime : TimeSpan.Zero;
            TimeSpan? cropEndTime = _downloadParams.CropEnd ? _downloadParams.CropEndTime : _downloadParams.Video.Length;

            string fileName = _filenameService.SubstituteWildcards(currentPrefs.DownloadFileName, _downloadParams.Video, _downloadParams.Quality, cropStartTime, cropEndTime);
            fileName = _filenameService.EnsureExtension(fileName, currentPrefs.DownloadDisableConversion);

            _downloadParams.Filename = fileName;
        }

        private void Download()
        {
            try
            {
                lock (_commandLockObject)
                {
                    Validate();

                    if (!HasErrors)
                    {
                        if (File.Exists(_downloadParams.FullPath))
                        {
                            MessageBoxResult result = _dialogService.ShowMessageBox("The file already exists. Do you want to overwrite it?", "Download", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (result != MessageBoxResult.Yes)
                            {
                                return;
                            }
                        }

                        _twitchService.Enqueue(_downloadParams);
                        _navigationService.NavigateBack();
                        _notificationService.ShowNotification("Download added");
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void Cancel()
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

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(DownloadParams);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                DownloadParams?.Validate();

                if (_downloadParams.FullPath.Length > 250)
                {
                    DownloadParams.AddError(nameof(DownloadParams.Filename), "The full filename (directory + filename) must be shorter than 250 characters!");
                }

                if (_twitchService.IsFileNameUsed(_downloadParams.FullPath))
                {
                    DownloadParams.AddError(nameof(DownloadParams.Filename), "Another video is already being downloaded to this file!");
                }

                if (DownloadParams.HasErrors)
                {
                    AddError(currentProperty, "Invalid Download Parameters!");

                    if (DownloadParams.GetErrors(nameof(DownloadParameters.CropStartTime)) is List<string> cropStartErrors && cropStartErrors.Count > 0)
                    {
                        string firstError = cropStartErrors.First();
                        AddError(nameof(CropStartHours), firstError);
                        AddError(nameof(CropStartMinutes), firstError);
                        AddError(nameof(CropStartSeconds), firstError);
                    }

                    if (DownloadParams.GetErrors(nameof(DownloadParameters.CropEndTime)) is List<string> cropEndErrors && cropEndErrors.Count > 0)
                    {
                        string firstError = cropEndErrors.First();
                        AddError(nameof(CropEndHours), firstError);
                        AddError(nameof(CropEndMinutes), firstError);
                        AddError(nameof(CropEndSeconds), firstError);
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

            menuCommands.Add(new MenuCommand(DownloadCommand, "Download", "Download"));
            menuCommands.Add(new MenuCommand(CancelCommand, "Cancel", "Times"));

            return menuCommands;
        }

        #endregion Methods

        #region EventHandlers

        private void _downloadParams_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_useCustomFilename)
            {
                return;
            }

            if (e.PropertyName == nameof(DownloadParameters.Quality)
                || e.PropertyName == nameof(DownloadParameters.CropStart)
                || e.PropertyName == nameof(DownloadParameters.CropEnd)
                || e.PropertyName == nameof(DownloadParameters.CropStartTime)
                || e.PropertyName == nameof(DownloadParameters.CropEndTime))
            {
                UpdateFilenameFromTemplate();
            }
        }

        #endregion EventHandlers
    }
}