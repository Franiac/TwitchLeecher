namespace TwitchLeecher.Services.Services.Download
{
    public class DownloadFileInfo
    {
        #region Constructors

        public DownloadFileInfo(string url, string localFile)
        {
            Url = url;
            LocalFile = localFile;
        }

        #endregion Constructors

        #region Properties

        public string Url { get; }

        public string LocalFile { get; }

        #endregion Properties
    }
}