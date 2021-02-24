using System;
using System.Text;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideoDownload : BindableBase
    {
        #region Fields

        private DownloadState _downloadState;
        private readonly object _downloadStateLockObject;

        private readonly StringBuilder _log;
        private readonly object _logLockObject;

        private double _progress;
        private readonly object _progressLockObject;

        private string _status;
        private readonly object _statusLockObject;

        private bool _isIndeterminate;
        private readonly object _isIndeterminateLockObject;

        #endregion Fields

        #region Constructors

        public TwitchVideoDownload(DownloadParameters downloadParams)
        {
            Id = Guid.NewGuid().ToString();
            DownloadParams = downloadParams ?? throw new ArgumentNullException(nameof(downloadParams));

            _log = new StringBuilder();

            _downloadStateLockObject = new object();
            _logLockObject = new object();
            _progressLockObject = new object();
            _statusLockObject = new object();
            _isIndeterminateLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public string Id { get; }

        public DownloadParameters DownloadParams { get; }

        public DownloadState DownloadState
        {
            get
            {
                return _downloadState;
            }
            private set
            {
                _downloadState = value;
                FirePropertyChanged(nameof(DownloadState));
                FirePropertyChanged(nameof(CanRetry));
                FirePropertyChanged(nameof(Status));
            }
        }

        public bool CanRetry
        {
            get
            {
                return DownloadState == DownloadState.Canceled || DownloadState == DownloadState.Error;
            }
        }

        public string Log
        {
            get
            {
                lock (_logLockObject)
                {
                    return _log.ToString();
                }
            }
        }

        public double Progress
        {
            get
            {
                return _progress;
            }
            private set
            {
                SetProperty(ref _progress, value);
            }
        }

        public string Status
        {
            get
            {
                if (_downloadState != DownloadState.Downloading)
                {
                    return _downloadState.ToString();
                }

                return _status;
            }
            private set
            {
                SetProperty(ref _status, value);
            }
        }

        public bool IsIndeterminate
        {
            get
            {
                return _isIndeterminate;
            }
            private set
            {
                SetProperty(ref _isIndeterminate, value);
            }
        }

        #endregion Properties

        #region Methods

        public void SetDownloadState(DownloadState downloadState)
        {
            lock (_downloadStateLockObject)
            {
                DownloadState = downloadState;
            }
        }

        public void AppendLog(string text)
        {
            lock (_logLockObject)
            {
                _log.Append(text);
                FirePropertyChanged(nameof(Log));
            }
        }

        public void ResetLog()
        {
            lock (_logLockObject)
            {
                _log.Clear();
                FirePropertyChanged(nameof(Log));
            }
        }

        public void SetProgress(double progress)
        {
            lock (_progressLockObject)
            {
                Progress = progress;
            }
        }

        public void SetStatus(string status)
        {
            lock (_statusLockObject)
            {
                Status = status;
            }
        }

        public void SetIsIndeterminate(bool isIndeterminate)
        {
            lock (_isIndeterminateLockObject)
            {
                IsIndeterminate = isIndeterminate;
            }
        }

        #endregion Methods
    }
}