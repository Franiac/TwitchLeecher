using System;
using System.Globalization;

namespace TwitchLeecher.Services.Models
{
    public class WebChunk
    {
        public WebChunk(string downloadUrl, string extInf, string localFile)
        {
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                throw new ArgumentNullException(nameof(downloadUrl));
            }

            if (string.IsNullOrWhiteSpace(extInf))
            {
                throw new ArgumentNullException(nameof(extInf));
            }

            if (string.IsNullOrWhiteSpace(localFile))
            {
                throw new ArgumentNullException(nameof(localFile));
            }

            this.DownloadUrl = downloadUrl;
            this.ExtInf = extInf;
            this.LocalFile = localFile;
        }

        public string DownloadUrl { get; private set; }

        public string ExtInf { get; private set; }

        public string LocalFile { get; private set; }

        public double Length
        {
            get
            {
                return double.Parse(this.ExtInf.Substring(this.ExtInf.LastIndexOf(":") + 1).TrimEnd(','), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
            }
        }
    }
}