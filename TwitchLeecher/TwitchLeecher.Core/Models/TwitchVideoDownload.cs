using System;
using System.Text;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideoDownload : BindableBase
    {
        #region Fields

        private string _id;

        private DownloadParameters _downloadParams;

        private DownloadStatus _downloadStatus;
        private object _downloadStatusLockObject;

        private StringBuilder _log;
        private object _logLockObject;

        private int _progress;
        private object _progressLockObject;

        private string _status;
        private object _statusLockObject;

        private bool _isEncoding;
        private object _isEncodingLockObject;

        #endregion Fields

        #region Constructors

        public TwitchVideoDownload(DownloadParameters downloadParams)
        {
            _id = Guid.NewGuid().ToString();
            _downloadParams = downloadParams ?? throw new ArgumentNullException(nameof(downloadParams));

            _log = new StringBuilder();

            _downloadStatusLockObject = new object();
            _logLockObject = new object();
            _progressLockObject = new object();
            _statusLockObject = new object();
            _isEncodingLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public string Id
        {
            get
            {
                return _id;
            }
        }

        public DownloadParameters DownloadParams
        {
            get
            {
                return _downloadParams;
            }
        }

        public DownloadStatus DownloadStatus
        {
            get
            {
                return _downloadStatus;
            }
            private set
            {
                _downloadStatus = value;
                FirePropertyChanged(nameof(DownloadStatus));
                FirePropertyChanged(nameof(Status));
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

        public int Progress
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
                if (_downloadStatus != DownloadStatus.Active)
                {
                    return _downloadStatus.ToString();
                }

                return _status;
            }
            private set
            {
                SetProperty(ref _status, value);
            }
        }

        public bool IsEncoding
        {
            get
            {
                return _isEncoding;
            }
            private set
            {
                SetProperty(ref _isEncoding, value);
            }
        }

        #endregion Properties

        #region Methods

        public void SetDownloadStatus(DownloadStatus downloadStatus)
        {
            lock (_downloadStatusLockObject)
            {
                DownloadStatus = downloadStatus;
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

        public void SetProgress(int progress)
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

        public void SetIsEncoding(bool isEncoding)
        {
            lock (_isEncodingLockObject)
            {
                IsEncoding = isEncoding;
            }
        }

        #endregion Methods
    }
}