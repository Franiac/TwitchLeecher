using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Core.Models
{
    public class SearchParameters
    {
        #region Fields

        private string username;
        private VideoType videoType;
        private int loadLimit;

        #endregion Fields

        #region Constructors

        public SearchParameters() : this(null, VideoType.Broadcast, 10)
        {
        }

        public SearchParameters(string username, VideoType videoType, int loadLimit)
        {
            this.username = username;
            this.videoType = videoType;
            this.loadLimit = loadLimit;
        }

        #endregion Constructors

        #region Properties

        public string Username
        {
            get
            {
                return this.username;
            }
        }

        public VideoType VideoType
        {
            get
            {
                return this.videoType;
            }
        }

        public int LoadLimit
        {
            get
            {
                return this.loadLimit;
            }
        }

        #endregion Properties
    }
}