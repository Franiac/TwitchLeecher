using System;

namespace TwitchLeecher.Core.Models
{
    public class UpdateInfo
    {
        #region Fields

        private Version newVersion;
        private DateTime releaseDate;
        private string downloadUrl;
        private string releaseNotes;

        #endregion Fields

        #region Constructors

        public UpdateInfo(Version newVersion, DateTime releaseDate, string downloadUrl, string releaseNotes)
        {
            if (newVersion == null)
            {
                throw new ArgumentNullException(nameof(newVersion));
            }

            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                throw new ArgumentNullException(nameof(downloadUrl));
            }

            if (string.IsNullOrWhiteSpace(releaseNotes))
            {
                throw new ArgumentNullException(nameof(releaseNotes));
            }

            this.newVersion = newVersion;
            this.releaseDate = releaseDate;
            this.downloadUrl = downloadUrl;
            this.releaseNotes = releaseNotes;
        }

        #endregion Constructors

        #region Properties

        public Version NewVersion
        {
            get
            {
                return this.newVersion;
            }
        }

        public DateTime ReleaseDate
        {
            get
            {
                return this.releaseDate;
            }
        }

        public string DownloadUrl
        {
            get
            {
                return this.downloadUrl;
            }
        }

        public string ReleaseNotes
        {
            get
            {
                return this.releaseNotes;
            }
        }

        #endregion Properties
    }
}