using System;
using System.Globalization;

namespace TwitchLeecher.Core.Models
{
    public class VodPlaylistPartExt : IVodPlaylistPartExt
    {
        #region Fields

        private int index;
        private string output;
        private string urlPrefix;
        private string localFile;
        private string downloadUrl;
        private double length;

        #endregion Fields

        #region Constructors

        public VodPlaylistPartExt(int index, string extinf, string remoteFile, string urlPrefix, string localFile)
        {
            if (string.IsNullOrWhiteSpace(extinf))
            {
                throw new ArgumentNullException(nameof(extinf));
            }

            if (string.IsNullOrWhiteSpace(remoteFile))
            {
                throw new ArgumentNullException(nameof(remoteFile));
            }

            if (string.IsNullOrWhiteSpace(urlPrefix))
            {
                throw new ArgumentNullException(nameof(urlPrefix));
            }

            if (string.IsNullOrWhiteSpace(localFile))
            {
                throw new ArgumentNullException(nameof(localFile));
            }

            this.index = index;
            this.downloadUrl = urlPrefix + remoteFile;
            this.length = double.Parse(extinf.Substring(extinf.LastIndexOf(":") + 1).TrimEnd(','), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            this.urlPrefix = urlPrefix;
            this.localFile = localFile;

            this.output = extinf + "\n" + localFile;
        }

        #endregion Constructors

        #region Properties

        public int Index
        {
            get
            {
                return this.index;
            }
        }

        public string DownloadUrl
        {
            get
            {
                return this.downloadUrl;
            }
        }

        public string LocalFile
        {
            get
            {
                return this.localFile;
            }
        }

        public double Length
        {
            get
            {
                return this.length;
            }
        }

        #endregion Properties

        #region Methods

        public string GetOutput()
        {
            return this.output;
        }

        #endregion Methods
    }
}