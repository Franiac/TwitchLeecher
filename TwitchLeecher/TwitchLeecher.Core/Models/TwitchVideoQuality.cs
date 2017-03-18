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

            QualityId = id;
            Resolution = resolution;
            Fps = fps;

            Initialize();
        }

        #endregion Constructors

        #region Properties

        public string QualityId { get; private set; }

        public string QualityFormatted { get; private set; }

        public int QualityPriority { get; private set; }

        public string Resolution { get; private set; }

        public string Fps { get; private set; }

        public string DisplayStringShort { get; private set; }

        public string DisplayStringLong { get; private set; }

        #endregion Properties

        #region Methods

        private void Initialize()
        {
            InitializeQuality();
            InitializeResolution();

            string quality = QualityId;
            string resolution = Resolution;
            string fps = Fps;

            if (quality.EndsWith("p", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(resolution) && !string.IsNullOrWhiteSpace(fps))
                {
                    DisplayStringShort = resolution + "@" + fps + "fps";
                }
                else if (!string.IsNullOrWhiteSpace(resolution) && string.IsNullOrWhiteSpace(fps))
                {
                    DisplayStringShort = resolution;
                }
                else
                {
                    DisplayStringShort = UNKNOWN;
                }

                DisplayStringLong = DisplayStringShort;
            }
            else
            {
                if (quality == "audio_only")
                {
                    DisplayStringShort = QualityFormatted;
                    DisplayStringLong = QualityFormatted;
                }
                else if (!string.IsNullOrWhiteSpace(resolution) && !string.IsNullOrWhiteSpace(fps))
                {
                    DisplayStringShort = resolution + "@" + fps + "fps";
                    DisplayStringLong = string.Format("{0} ({1})", QualityFormatted, DisplayStringShort);
                }
                else if (!string.IsNullOrWhiteSpace(resolution) && string.IsNullOrWhiteSpace(fps))
                {
                    DisplayStringShort = resolution;
                    DisplayStringLong = string.Format("{0} ({1})", QualityFormatted, DisplayStringShort);
                }
                else
                {
                    DisplayStringShort = UNKNOWN;
                    DisplayStringLong = UNKNOWN;
                }
            }
        }

        public void InitializeQuality()
        {
            string qualityId = QualityId;

            QualityFormatted = GetQualityFormatted(qualityId);

            switch (qualityId.ToLowerInvariant())
            {
                case QUALITY_SOURCE:
                    QualityPriority = 0;
                    break;

                case QUALITY_HIGH:
                    QualityPriority = 1;
                    break;

                case QUALITY_MEDIUM:
                    QualityPriority = 2;
                    break;

                case QUALITY_LOW:
                    QualityPriority = 3;
                    break;

                case QUALITY_MOBILE:
                    QualityPriority = 4;
                    break;

                case QUALITY_AUDIO:
                    QualityPriority = 5;
                    break;

                case QUALITY_1080P:
                    QualityPriority = 10;
                    break;

                case QUALITY_720P:
                    QualityPriority = 11;
                    break;

                case QUALITY_480P:
                    QualityPriority = 12;
                    break;

                case QUALITY_360P:
                    QualityPriority = 13;
                    break;

                case QUALITY_240P:
                    QualityPriority = 14;
                    break;

                case QUALITY_144P:
                    QualityPriority = 15;
                    break;

                default:
                    QualityPriority = 100;
                    break;
            }
        }

        private void InitializeResolution()
        {
            string qualityId = QualityId;
            string resolution = Resolution;

            if (!string.IsNullOrWhiteSpace(resolution) && resolution.Equals("0x0", StringComparison.OrdinalIgnoreCase))
            {
                switch (qualityId)
                {
                    case QUALITY_HIGH:
                        Resolution = "1280x720";
                        break;

                    case QUALITY_MEDIUM:
                        Resolution = "852x480";
                        break;

                    case QUALITY_LOW:
                        Resolution = "640x360";
                        break;

                    case QUALITY_MOBILE:
                        Resolution = "400x226";
                        break;

                    default:
                        Resolution = null;
                        break;
                }
            }
        }

        public override string ToString()
        {
            return DisplayStringLong;
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