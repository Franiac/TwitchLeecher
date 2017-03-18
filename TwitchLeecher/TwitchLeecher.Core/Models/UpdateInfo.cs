using System;

namespace TwitchLeecher.Core.Models
{
    public class UpdateInfo
    {
        #region Fields

        private Version _newVersion;
        private DateTime _releaseDate;
        private string _downloadUrl;
        private string _releaseNotes;

        #endregion Fields

        #region Constructors

        public UpdateInfo(Version newVersion, DateTime releaseDate, string downloadUrl, string releaseNotes)
        {
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                throw new ArgumentNullException(nameof(downloadUrl));
            }

            if (string.IsNullOrWhiteSpace(releaseNotes))
            {
                throw new ArgumentNullException(nameof(releaseNotes));
            }

            _newVersion = newVersion ?? throw new ArgumentNullException(nameof(newVersion));
            _releaseDate = releaseDate;
            _downloadUrl = downloadUrl;
            _releaseNotes = releaseNotes;
        }

        #endregion Constructors

        #region Properties

        public Version NewVersion
        {
            get
            {
                return _newVersion;
            }
        }

        public DateTime ReleaseDate
        {
            get
            {
                return _releaseDate;
            }
        }

        public string DownloadUrl
        {
            get
            {
                return _downloadUrl;
            }
        }

        public string ReleaseNotes
        {
            get
            {
                return _releaseNotes;
            }
        }

        #endregion Properties
    }
}