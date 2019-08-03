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
                FirePropertyChanged(nameof(AutoSplitPartCount));
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
                FirePropertyChanged(nameof(AutoSplitPartCount));
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
                FirePropertyChanged(nameof(AutoSplitPartCount));
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
                FirePropertyChanged(nameof(AutoSplitPartCount));
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
                FirePropertyChanged(nameof(AutoSplitPartCount));
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
                FirePropertyChanged(nameof(AutoSplitPartCount));
            }
        }

        public bool AutoSplitUseExtended
        {
            get
            {
                return _downloadParams.AutoSplit;
            }
            set
            {
                if (value != _downloadParams.AutoSplit)
                {
                    _downloadParams.AutoSplit = value;
                    if (_downloadParams.AutoSplit)
                    {
                        if (!UseCustomFilename)
                            UpdateFilenameFromTemplate();
                        else if (!_downloadParams.Filename.Contains(FilenameWildcards.UNIQNUMBER))
                            _downloadParams.Filename = Path.Combine(Path.GetDirectoryName(_downloadParams.Filename), $"{Path.GetFileNameWithoutExtension(_downloadParams.Filename)}_{FilenameWildcards.UNIQNUMBER}{Path.GetExtension(_downloadParams.Filename)}");
                    }
                    else if (_downloadParams.Filename.Contains(FilenameWildcards.UNIQNUMBER))
                    {
                        if (!UseCustomFilename)
                            UpdateFilenameFromTemplate();
                        else
                            _downloadParams.Filename = _downloadParams.Filename.Replace("_" + FilenameWildcards.UNIQNUMBER, "").Replace(FilenameWildcards.UNIQNUMBER, "");
                    }
                    FirePropertyChanged(nameof(AutoSplitUseExtended));
                }
            }
        }

        public int AutoSplitTimeHours
        {
            get
            {
                return _downloadParams.AutoSplitTime.GetDaysInHours();
            }
            set
            {
                TimeSpan current = _downloadParams.AutoSplitTime;
                _downloadParams.AutoSplitTime = new TimeSpan(value, current.Minutes, current.Seconds);

                FirePropertyChanged(nameof(AutoSplitTimeHours));
                FirePropertyChanged(nameof(AutoSplitTimeMinutes));
                FirePropertyChanged(nameof(AutoSplitTimeSeconds));
                FirePropertyChanged(nameof(AutoSplitPartCount));
            }
        }

        public int AutoSplitTimeMinutes
        {
            get
            {
                return _downloadParams.AutoSplitTime.Minutes;
            }
            set
            {
                TimeSpan current = _downloadParams.AutoSplitTime;
                _downloadParams.AutoSplitTime = new TimeSpan(current.GetDaysInHours(), value, current.Seconds);

                FirePropertyChanged(nameof(AutoSplitTimeHours));
                FirePropertyChanged(nameof(AutoSplitTimeMinutes));
                FirePropertyChanged(nameof(AutoSplitTimeSeconds));
                FirePropertyChanged(nameof(AutoSplitPartCount));
            }
        }

        public int AutoSplitTimeSeconds
        {
            get
            {
                return _downloadParams.AutoSplitTime.Seconds;
            }
            set
            {
                TimeSpan current = _downloadParams.AutoSplitTime;
                _downloadParams.AutoSplitTime = new TimeSpan(current.GetDaysInHours(), current.Minutes, value);

                FirePropertyChanged(nameof(AutoSplitTimeHours));
                FirePropertyChanged(nameof(AutoSplitTimeMinutes));
                FirePropertyChanged(nameof(AutoSplitTimeSeconds));
                FirePropertyChanged(nameof(AutoSplitPartCount));
            }
        }

        public int AutoSplitPartCount
        {
            get
            {
                if (_downloadParams.AutoSplit == false || DownloadParams.AutoSplitTime.TotalSeconds < 60 || DownloadParams.CroppedLength.TotalSeconds < 60)
                    return 1;
                return Math.Max((int)Math.Ceiling((DownloadParams.CroppedLength.TotalSeconds - 60) / DownloadParams.AutoSplitTime.TotalSeconds), 1);
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
            string folder = currentPrefs.DownloadFolder;
            string fileName = currentPrefs.DownloadFileName;

            TimeSpan? cropStartTime = _downloadParams.CropStart ? _downloadParams.CropStartTime : TimeSpan.Zero;
            TimeSpan? cropEndTime = _downloadParams.CropEnd ? _downloadParams.CropEndTime : _downloadParams.Video.Length;

            fileName = _filenameService.EnsureExtension(fileName, currentPrefs.DownloadDisableConversion);

            if (AutoSplitUseExtended && fileName.Contains(FilenameWildcards.UNIQNUMBER))
            {
                fileName = fileName.Replace(FilenameWildcards.UNIQNUMBER, "{UNIQNUMBERTEMP}");
                fileName = _filenameService.SubstituteWildcards(fileName, folder, _twitchService.IsFileNameUsed, _downloadParams.Video, _downloadParams.Quality, cropStartTime, cropEndTime);
                fileName = fileName.Replace("{UNIQNUMBERTEMP}", FilenameWildcards.UNIQNUMBER);
            }
            else
                fileName = _filenameService.SubstituteWildcards(fileName, folder, _twitchService.IsFileNameUsed, _downloadParams.Video, _downloadParams.Quality, cropStartTime, cropEndTime);


            _downloadParams.Filename = fileName;
        }

        private string GetFilenameFromTemplate(string fileName, string folder, TimeSpan? cropStartTime = null, TimeSpan? cropEndTime = null)
        {
            return _filenameService.SubstituteWildcards(fileName, folder, _twitchService.IsFileNameUsed, _downloadParams.Video, _downloadParams.Quality, cropStartTime, cropEndTime);
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
                        int downloadAddedCount = 1;
                        if (_downloadParams.AutoSplit && _downloadParams.AutoSplitTime.TotalSeconds > 60)
                        {
                            string baseFolder = Path.GetDirectoryName(_downloadParams.FullPath);
                            if (!baseFolder.EndsWith("\\")) baseFolder = baseFolder + "\\";
                            string baseFilename = _downloadParams.FullPath.Replace(baseFolder, "");
                            if (baseFilename.StartsWith("\\")) baseFilename = baseFilename.Substring(1);

                            if (!UseCustomFilename)
                            {
                                baseFilename = _preferencesService.CurrentPreferences.DownloadFileName;
                                baseFilename = _filenameService.EnsureExtension(baseFilename, _preferencesService.CurrentPreferences.DownloadDisableConversion);
                                baseFolder = _preferencesService.CurrentPreferences.DownloadFolder;
                            }
                            if (!baseFilename.Contains(FilenameWildcards.UNIQNUMBER))
                            {
                                baseFilename = Path.GetFileNameWithoutExtension(baseFilename) + "_" + FilenameWildcards.UNIQNUMBER + Path.GetExtension(baseFilename);
                            }
                            TimeSpan start = _downloadParams.CropStart ? _downloadParams.CropStartTime : TimeSpan.Zero;
                            TimeSpan end = start + _downloadParams.AutoSplitTime.Add(new TimeSpan(0, 0, 10));
                            while (end < (_downloadParams.CropEnd ? _downloadParams.CropEndTime : _downloadParams.Video.Length))
                            {
                                string filename = GetFilenameFromTemplate(baseFilename, baseFolder, start, end);
                                DownloadParameters tempParams = new DownloadParameters(_downloadParams.Video, _downloadParams.VodAuthInfo, _downloadParams.Quality, baseFolder, filename, _downloadParams.AutoSplitTime, _downloadParams.DisableConversion);
                                tempParams.AutoSplit = false;
                                if (start.Ticks > 0)
                                {
                                    tempParams.CropStart = true;
                                    tempParams.CropStartTime = start;
                                }
                                else
                                    tempParams.CropStart = false;
                                tempParams.CropEnd = true;
                                tempParams.CropEndTime = end;
                                start = end.Add(new TimeSpan(0, 0, -10));
                                end = start + _downloadParams.AutoSplitTime.Add(new TimeSpan(0, 0, 10));
                                _twitchService.Enqueue(tempParams);
                                downloadAddedCount++;
                            }
                            _downloadParams.AutoSplit = false;
                            end = (_downloadParams.CropEnd ? _downloadParams.CropEndTime : _downloadParams.Video.Length);
                            if (start.Ticks > 0)
                            {
                                _downloadParams.CropStart = true;
                                _downloadParams.CropStartTime = start;
                            }
                            else
                                _downloadParams.CropStart = false;

                            _downloadParams.Filename = GetFilenameFromTemplate(baseFilename, baseFolder, start, end);

                            _twitchService.Enqueue(_downloadParams);
                        }
                        else
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
                        }
                        _navigationService.NavigateBack();
                        if (downloadAddedCount <= 1)
                            _notificationService.ShowNotification("Download added");
                        else
                            _notificationService.ShowNotification($"{downloadAddedCount} downloads added");
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

                    if (DownloadParams.GetErrors(nameof(DownloadParameters.AutoSplitTime)) is List<string> autoSplitTimeErrors && autoSplitTimeErrors.Count > 0)
                    {
                        string firstError = autoSplitTimeErrors.First();
                        AddError(nameof(AutoSplitTimeHours), firstError);
                        AddError(nameof(AutoSplitTimeMinutes), firstError);
                        AddError(nameof(AutoSplitTimeSeconds), firstError);
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