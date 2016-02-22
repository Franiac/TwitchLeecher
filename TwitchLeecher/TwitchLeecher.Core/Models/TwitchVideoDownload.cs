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

        private StringBuilder log;
        private object logLockObject;
        private int progress;
        private string status;

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

            this.logLockObject = new object();
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
            set
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
            set
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
            set
            {
                this.status = value;
                this.FirePropertyChanged(nameof(this.Status));
            }
        }

        public TwitchVideo Video
        {
            get
            {
                return this.downloadParams.Video;
            }
        }

        #endregion Properties

        #region Methods

        public void AppendLog(string text)
        {
            lock (logLockObject)
            {
                this.log.Append(text);
                this.FirePropertyChanged(nameof(this.Log));
            }
        }

        public void FirePropertyChanged(string propertyName = null)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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

        #endregion Methods
    }
}