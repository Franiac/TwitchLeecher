using System;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideoQuality
    {
        #region Constants

        public const string QUALITY_SOURCE = "chunked";
        public const string QUALITY_HIGH = "high";
        public const string QUALITY_MEDIUM = "medium";
        public const string QUALITY_LOW = "low";
        public const string QUALITY_MOBILE = "mobile";
        public const string QUALITY_AUDIO = "audio_only";

        public const string QUALITY_1080P = "1080p";
        public const string QUALITY_720P = "720p";
        public const string QUALITY_480P = "480p";
        public const string QUALITY_360P = "360p";
        public const string QUALITY_240P = "240p";
        public const string QUALITY_144P = "144p";

        public const string UNKNOWN = "Unknown";

        #endregion Constants

        #region Constructors

        public TwitchVideoQuality(string id, string resolution = null, string fps = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            this.QualityId = id;
            this.Resolution = resolution;
            this.Fps = fps;

            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public string QualityId { get; private set; }

        public string QualityFormatted { get; private set; }

        public string Resolution { get; private set; }

        public string Fps { get; private set; }

        public string DisplayStringShort { get; private set; }

        public string DisplayStringLong { get; private set; }

        #endregion Properties

        #region Methods

        private void Initialize()
        {
            this.QualityFormatted = GetQualityFormatted(this.QualityId);

            string resolution = this.Resolution;
            string fps = this.Fps;

            if (this.QualityId.EndsWith("p", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(resolution) && !string.IsNullOrWhiteSpace(fps))
                {
                    this.DisplayStringShort = resolution + "@" + fps + "fps";
                }
                else if (!string.IsNullOrWhiteSpace(resolution) && string.IsNullOrWhiteSpace(fps))
                {
                    this.DisplayStringShort = resolution;
                }
                else
                {
                    this.DisplayStringShort = UNKNOWN;
                }

                this.DisplayStringLong = this.DisplayStringShort;
            }
            else
            {
                if (this.QualityId == "audio_only")
                {
                    this.DisplayStringShort = this.QualityFormatted;
                    this.DisplayStringLong = this.QualityFormatted;
                }
                else if (!string.IsNullOrWhiteSpace(resolution) && !string.IsNullOrWhiteSpace(fps))
                {
                    this.DisplayStringShort = resolution + "@" + fps + "fps";
                    this.DisplayStringLong = string.Format("{0} ({1})", this.QualityFormatted, this.DisplayStringShort);
                }
                else if (!string.IsNullOrWhiteSpace(resolution) && string.IsNullOrWhiteSpace(fps))
                {
                    this.DisplayStringShort = resolution;
                    this.DisplayStringLong = string.Format("{0} ({1})", this.QualityFormatted, this.DisplayStringShort);
                }
                else
                {
                    this.DisplayStringShort = UNKNOWN;
                    this.DisplayStringLong = UNKNOWN;
                }
            }
        }

        public override string ToString()
        {
            return this.DisplayStringLong;
        }

        #endregion Methods

        #region Static Methods

        public static string GetQualityFormatted(string qualityId)
        {
            switch (qualityId.ToLowerInvariant())
            {
                case QUALITY_SOURCE:
                    return "Source";

                case QUALITY_HIGH:
                    return "High";

                case QUALITY_MEDIUM:
                    return "Medium";

                case QUALITY_LOW:
                    return "Low";

                case QUALITY_MOBILE:
                    return "Mobile";

                case QUALITY_AUDIO:
                    return "Audio Only";

                default:
                    return qualityId;
            }
        }

        #endregion Static Methods
    }
}