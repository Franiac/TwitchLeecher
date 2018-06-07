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
using TwitchLeecher.Shared.Extensions;

namespace TwitchLeecher.Gui.ViewModels
{
    public class DownloadViewVM : ViewModelBase
    {
        #region Fields

        private DownloadParameters _downloadParams;

        private ICommand _chooseCommand;
        private ICommand _downloadCommand;
        private ICommand _cancelCommand;

        private readonly IDialogService _dialogService;
        private readonly ITwitchService _twitchService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructors

        public DownloadViewVM(
            IDialogService dialogService,
            ITwitchService twitchService,
            INavigationService navigationService,
            INotificationService notificationService)
        {
            _dialogService = dialogService;
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
                SetProperty(ref _downloadParams, value, nameof(DownloadParams));
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
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
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
    }
}