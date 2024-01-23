using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class DownloadsViewModel : ViewModelBase, INavigationState
    {
        #region Fields

        private readonly IDialogService _dialogService;
        private readonly IDownloadService _downloadService;
        private readonly INavigationService _navigationService;

        private ICommand _retryDownloadCommand;
        private ICommand _cancelDownloadCommand;
        private ICommand _removeDownloadCommand;
        private ICommand _showLogCommand;
        private ICommand _openDownloadFolderCommand;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructors

        public DownloadsViewModel(
            IDialogService dialogService,
            IDownloadService downloadService,
            INavigationService navigationService)
        {
            _dialogService = dialogService;
            _downloadService = downloadService;
            _navigationService = navigationService;

            _downloadService.PropertyChanged += DownloadService_PropertyChanged;

            _commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public double ScrollPosition { get; set; }

        public ObservableCollection<TwitchVideoDownload> Downloads
        {
            get { return _downloadService.Downloads; }
        }

        public ICommand RetryDownloadCommand
        {
            get
            {
                if (_retryDownloadCommand == null)
                {
                    _retryDownloadCommand = new DelegateCommand<string>(RetryDownload);
                }

                return _retryDownloadCommand;
            }
        }

        public ICommand CancelDownloadCommand
        {
            get
            {
                if (_cancelDownloadCommand == null)
                {
                    _cancelDownloadCommand = new DelegateCommand<string>(CancelDownload);
                }

                return _cancelDownloadCommand;
            }
        }


        public ICommand ShowLogCommand
        {
            get
            {
                if (_showLogCommand == null)
                {
                    _showLogCommand = new DelegateCommand<string>(ShowLog);
                }

                return _showLogCommand;
            }
        }

        public ICommand OpenDownloadFolderCommand
        {
            get
            {
                if (_openDownloadFolderCommand == null)
                {
                    _openDownloadFolderCommand = new DelegateCommand<string>(OpenDownloadFolder);
                }

                return _openDownloadFolderCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void RetryDownload(string id)
        {
            try
            {
                lock (_commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        _downloadService.Retry(id);
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void CancelDownload(string id)
        {
            try
            {
                lock (_commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        if (Downloads.Single(d => d.Id == id).DownloadState == DownloadState.Downloading)
                        {
                            _downloadService.Cancel(id);
                            return;
                        }

                        _downloadService.Remove(id);
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ShowLog(string id)
        {
            try
            {
                lock (_commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        TwitchVideoDownload download = Downloads.Where(d => d.Id == id).FirstOrDefault();

                        if (download != null)
                        {
                            _navigationService.ShowLog(download);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void OpenDownloadFolder(string id)
        {
            try
            {
                lock (_commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        TwitchVideoDownload download = Downloads.Where(d => d.Id == id).FirstOrDefault();

                        if (download != null)
                        {
                            string folder = download.DownloadParams.Folder;

                            if (Directory.Exists(folder))
                            {
                                Process.Start(folder);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        #endregion Methods

        #region EventHandlers

        private void DownloadService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            FirePropertyChanged(e.PropertyName);
        }

        #endregion EventHandlers
    }
}