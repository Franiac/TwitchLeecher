using System;
using System.Text;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideoDownload : BindableBase
    {
        #region Fields

        private DownloadParameters downloadParams;

        private DownloadStatus downloadStatus;
        private object downloadStatusLockObject;

        private StringBuilder log;
        private object logLockObject;

        private int progress;
        private object progressLockObject;

        private string status;
        private object statusLockObject;

        private bool isEncoding;
        private object isEncodingLockObject;

        #endregion Fields

        #region Constructors

        public TwitchVideoDownload(DownloadParameters downloadParams)
        {
            if (downloadParams == null)
            {
                throw new ArgumentNullException(nameof(downloadParams));
            }

            this.downloadParams = downloadParams;

            this.log = new StringBuilder();

            this.downloadStatusLockObject = new object();
            this.logLockObject = new object();
            this.progressLockObject = new object();
            this.statusLockObject = new object();
            this.isEncodingLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public DownloadParameters DownloadParams
        {
            get
            {
                return this.downloadParams;
            }
        }

        public DownloadStatus DownloadStatus
        {
            get
            {
                return this.downloadStatus;
            }
            private set
            {
                this.downloadStatus = value;
                this.FirePropertyChanged(nameof(this.DownloadStatus));
                this.FirePropertyChanged(nameof(this.Status));
            }
        }

        public string Log
        {
            get
            {
                lock (logLockObject)
                {
                    return this.log.ToString();
                }
            }
        }

        public int Progress
        {
            get
            {
                return this.progress;
            }
            private set
            {
                this.SetProperty(ref this.progress, value);
            }
        }

        public string Status
        {
            get
            {
                if (this.downloadStatus != DownloadStatus.Active)
                {
                    return this.downloadStatus.ToString();
                }

                return this.status;
            }
            private set
            {
                this.SetProperty(ref this.status, value);
            }
        }

        public bool IsEncoding
        {
            get
            {
                return this.isEncoding;
            }
            private set
            {
                this.SetProperty(ref this.isEncoding, value);
            }
        }

        #endregion Properties

        #region Methods

        public void SetDownloadStatus(DownloadStatus downloadStatus)
        {
            lock (this.downloadStatusLockObject)
            {
                this.DownloadStatus = downloadStatus;
            }
        }

        public void AppendLog(string text)
        {
            lock (this.logLockObject)
            {
                this.log.Append(text);
                this.FirePropertyChanged(nameof(this.Log));
            }
        }

        public void ResetLog()
        {
            lock (logLockObject)
            {
                this.log.Clear();
                this.FirePropertyChanged(nameof(this.Log));
            }
        }

        public void SetProgress(int progress)
        {
            lock (this.progressLockObject)
            {
                this.Progress = progress;
            }
        }

        public void SetStatus(string status)
        {
            lock (this.statusLockObject)
            {
                this.Status = status;
            }
        }

        public void SetIsEncoding(bool isEncoding)
        {
            lock (this.isEncodingLockObject)
            {
                this.IsEncoding = isEncoding;
            }
        }

        #endregion Methods
    }
}