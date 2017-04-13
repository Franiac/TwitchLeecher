namespace TwitchLeecher.Services.Services.Download
{
    public class DownloadFileInfo
    {
        #region Fields

        private string _url;
        private string _localFile;

        #endregion Fields

        #region Constructors

        public DownloadFileInfo(string url, string localFile)
        {
            _url = url;
            _localFile = localFile;
        }

        #endregion Constructors

        #region Properties

        public string Url => _url;
        public string LocalFile => _localFile;

        #endregion Properties
    }
}