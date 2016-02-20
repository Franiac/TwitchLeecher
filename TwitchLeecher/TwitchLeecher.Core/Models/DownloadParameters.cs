using System;

namespace TwitchLeecher.Core.Models
{
    public class DownloadParameters
    {
        #region Fields

        private TwitchVideo video;
        private TwitchVideoResolution resolution;
        private string filename;

        #endregion Fields

        #region Constructors

        public DownloadParameters(TwitchVideo video)
        {
            if (video == null)
            {
                throw new ArgumentNullException(nameof(video));
            }

            this.video = video;
        }

        public DownloadParameters(TwitchVideo video, TwitchVideoResolution resolution, string filename) : this(video)
        {
            if (resolution == null)
            {
                throw new ArgumentNullException(nameof(resolution));
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentNullException(nameof(filename));
            }

            this.resolution = resolution;
            this.filename = filename;
        }

        #endregion Constructors

        #region Properties

        public TwitchVideo Video
        {
            get
            {
                return this.video;
            }
        }

        public TwitchVideoResolution Resolution
        {
            get
            {
                return this.resolution;
            }
            set
            {
                this.resolution = value;
            }
        }

        public string Filename
        {
            get
            {
                return this.filename;
            }
            set
            {
                this.filename = value;
            }
        }

        #endregion Properties
    }
}