using System;

namespace TwitchLeecher.Services.Services.Download
{
    public class DownloadEventArgs : EventArgs
    {
        #region Fields

        private Download _download;

        #endregion Fields

        #region Constructors

        public DownloadEventArgs(Download download)
        {
            _download = download ?? throw new ArgumentNullException(nameof(download));
        }

        #endregion Constructors

        #region Properties

        public Download Download => _download;

        #endregion Properties
    }
}