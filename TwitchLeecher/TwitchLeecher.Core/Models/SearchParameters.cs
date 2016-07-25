using TwitchLeecher.Core.Enums;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class SearchParameters : BindableBase
    {
        #region Fields

        private string username;
        private VideoType videoType;
        private int loadLimit;

        #endregion Fields

        #region Constructors

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
            set
            {
                this.SetProperty(ref this.username, value, nameof(this.Username));
            }
        }

        public VideoType VideoType
        {
            get
            {
                return this.videoType;
            }
            set
            {
                this.SetProperty(ref this.videoType, value, nameof(this.VideoType));
            }
        }

        public int LoadLimit
        {
            get
            {
                return this.loadLimit;
            }
            set
            {
                this.SetProperty(ref this.loadLimit, value, nameof(this.LoadLimit));
            }
        }

        #endregion Properties

        #region Methods

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(this.Username);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(this.username))
                {
                    this.AddError(currentProperty, "Please specify a username!");
                }
            }
        }

        public SearchParameters Clone()
        {
            return new SearchParameters(this.Username, this.VideoType, this.LoadLimit);
        }

        #endregion Methods
    }
}