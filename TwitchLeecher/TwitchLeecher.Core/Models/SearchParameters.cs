using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Core.Models
{
    public class SearchParameters
    {
        #region Fields

        private string username;
        private VideoType videoType;
        private string loadLimit;

        #endregion Fields

        #region Constructors

        public SearchParameters() : this(null, VideoType.Broadcast, Preferences.DEFAULT_LOAD_LIMIT)
        {
        }

        public SearchParameters(string username, VideoType videoType, string loadLimit)
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

        public string LoadLimit
        {
            get
            {
                return this.loadLimit;
            }
        }

        #endregion Properties
    }
}