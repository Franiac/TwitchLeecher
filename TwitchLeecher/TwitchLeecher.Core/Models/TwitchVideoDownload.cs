using System;
using System.ComponentModel;
using System.Text;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideoDownload : INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;

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
                return this.log.ToString();
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
                this.progress = value;
                this.FirePropertyChanged(nameof(this.Progress));
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
                this.status = value;
                this.FirePropertyChanged(nameof(this.Status));
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

        public void FirePropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}