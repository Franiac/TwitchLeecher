using System;
using System.Globalization;

namespace TwitchLeecher.Core.Models
{
    public class VodPlaylistPartExt : IVodPlaylistPartExt
    {
        #region Fields

        private int _index;
        private string _output;
        private string _urlPrefix;
        private string _localFile;
        private string _downloadUrl;
        private double _length;

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

            _index = index;
            _downloadUrl = urlPrefix + remoteFile;
            _length = Math.Max(double.Parse(extinf.Substring(extinf.LastIndexOf(":") + 1).TrimEnd(','), NumberStyles.Any, CultureInfo.InvariantCulture), 0);
            _urlPrefix = urlPrefix;
            _localFile = localFile;

            _output = extinf + "\n" + localFile;
        }

        #endregion Constructors

        #region Properties

        public int Index
        {
            get
            {
                return _index;
            }
        }

        public string DownloadUrl
        {
            get
            {
                return _downloadUrl;
            }
        }

        public string LocalFile
        {
            get
            {
                return _localFile;
            }
        }

        public double Length
        {
            get
            {
                return _length;
            }
        }

        #endregion Properties

        #region Methods

        public string GetOutput()
        {
            return _output;
        }

        #endregion Methods
    }
}