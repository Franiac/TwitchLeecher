using System;

namespace TwitchLeecher.Services.Services.Download
{
    public class DownloadEventArgs : EventArgs
    {
        #region Constructors

        public DownloadEventArgs(Download download)
        {
            Download = download ?? throw new ArgumentNullException(nameof(download));
        }

        #endregion Constructors

        #region Properties

        public Download Download { get; }

        #endregion Properties
    }
}