using System.Linq;
using System.Reflection;
using TwitchLeecher.Core.Attributes;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideoResolution
    {
        #region Constants

        public const string UNKNOWN = "Unknown";

        #endregion Constants

        #region Fields

        private string videoQualityAsString;
        private string resolutionAsString;

        #endregion Fields

        #region Constructors

        public TwitchVideoResolution(VideoQuality videoQuality, string resolution, string fps)
        {
            this.VideoQuality = videoQuality;
            this.Resolution = resolution;
            this.Fps = fps;

            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public VideoQuality VideoQuality { get; private set; }

        public string Resolution { get; private set; }

        public string Fps { get; private set; }

        public string VideoQualityAsString
        {
            get
            {
                return this.videoQualityAsString;
            }
        }

        public string ResolutionAsString
        {
            get
            {
                return this.resolutionAsString;
            }
        }

        #endregion Properties

        #region Methods

        private void Initialize()
        {
            MemberInfo[] memberInfo = typeof(VideoQuality).GetMember(this.VideoQuality.ToString());
            object[] customAttributes = memberInfo.First().GetCustomAttributes(typeof(EnumDisplayNameAttribute), false);
            this.videoQualityAsString = ((EnumDisplayNameAttribute)customAttributes.First()).Name;

            string resolution = this.Resolution;
            string fps = this.Fps;

            if (!string.IsNullOrWhiteSpace(resolution) && !string.IsNullOrWhiteSpace(fps))
            {
                this.resolutionAsString = resolution + "@" + fps + "fps";
            }
            else if (!string.IsNullOrWhiteSpace(resolution) && string.IsNullOrWhiteSpace(fps))
            {
                this.resolutionAsString = resolution;
            }
            else if (this.VideoQuality == VideoQuality.AudioOnly)
            {
                this.resolutionAsString = this.videoQualityAsString;
            }
            else
            {
                this.resolutionAsString = UNKNOWN;
            }
        }

        public override string ToString()
        {
            return this.VideoQuality == VideoQuality.AudioOnly
                ? this.videoQualityAsString
                : string.Format("{0} ({1})", this.videoQualityAsString, this.resolutionAsString);
        }

        #endregion Methods
    }
}