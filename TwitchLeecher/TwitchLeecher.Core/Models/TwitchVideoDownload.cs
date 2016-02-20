using Prism.Mvvm;
using System;
using System.Text;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideoDownload : BindableBase
    {
        #region Fields

        private DownloadParameters downloadParams;

        private DownloadStatus downloadStatus;

        private string status;
        private int progress;

        private StringBuilder log;

        private object logLockObject;

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

        public TwitchVideo Video
        {
            get
            {
                return this.downloadParams.Video;
            }
        }

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
                this.SetProperty(ref this.downloadStatus, value, nameof(this.DownloadStatus));
                this.OnPropertyChanged(nameof(this.Status));
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
                this.SetProperty(ref this.status, value, nameof(this.Status));
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
                this.SetProperty(ref this.progress, value, nameof(this.Progress));
            }
        }

        public string Log
        {
            get
            {
                return this.log.ToString();
            }
        }

        #endregion Properties

        #region Methods

        public void AppendLog(string text)
        {
            lock (logLockObject)
            {
                this.log.Append(text);
                this.OnPropertyChanged(nameof(this.Log));
            }
        }

        public void ResetLog()
        {
            lock (logLockObject)
            {
                this.log.Clear();
                this.OnPropertyChanged(nameof(this.Log));
            }
        }

        #endregion Methods
    }
}