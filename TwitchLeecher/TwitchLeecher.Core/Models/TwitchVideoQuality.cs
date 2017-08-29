using System;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideoQuality : IComparable<TwitchVideoQuality>
    {
        #region Constants

        public const string UNKNOWN = "Unknown";

        public const string QUALITY_SOURCE = "chunked";
        public const string QUALITY_HIGH = "high";
        public const string QUALITY_MEDIUM = "medium";
        public const string QUALITY_LOW = "low";
        public const string QUALITY_MOBILE = "mobile";
        public const string QUALITY_AUDIO = "audio_only";

        #endregion Constants

        #region Constructors

        public TwitchVideoQuality(string qualityId, string resolution = null, string fps = null)
        {
            if (string.IsNullOrWhiteSpace(qualityId))
            {
                throw new ArgumentNullException(nameof(qualityId));
            }

            Initialize(qualityId, resolution, fps);
        }

        #endregion Constructors

        #region Properties

        public string QualityId { get; private set; }

        public string QualityString { get; private set; }

        public string DisplayString { get; private set; }

        public string ResFpsString { get; private set; }

        public string Resolution { get; private set; }

        public int? Fps { get; private set; }

        #endregion Properties

        #region Methods

        private void Initialize(string qualityId, string resolution, string fps)
        {
            QualityId = qualityId;
            QualityString = GetQualityString(qualityId);
            Resolution = GetResolution(qualityId, resolution);
            Fps = GetFps(qualityId, fps);

            if (qualityId == QUALITY_AUDIO)
            {
                DisplayString = QualityString;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(Resolution) && Fps.HasValue)
                {
                    DisplayString = Resolution + "@" + Fps + "fps" + (!string.IsNullOrWhiteSpace(QualityString) ? " (" + QualityString + ")" : null);
                }
                else if (!string.IsNullOrWhiteSpace(Resolution) && !Fps.HasValue)
                {
                    DisplayString = Resolution + (!string.IsNullOrWhiteSpace(QualityString) ? " (" + QualityString + ")" : null);
                }
                else
                {
                    DisplayString = UNKNOWN;
                }
            }

            string displayStr = DisplayString;
            int index = displayStr.IndexOf(" (");

            ResFpsString = index >= 0 ? displayStr.Substring(0, displayStr.Length - (displayStr.Length - index)) : displayStr;
        }

        public bool IsSource
        {
            get
            {
                return QualityId == QUALITY_SOURCE;
            }
        }

        public string GetQualityString(string qualityId)
        {
            switch (qualityId)
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
                    return null;
            }
        }

        private string GetResolution(string qualityId, string resolution)
        {
            if (!string.IsNullOrWhiteSpace(resolution))
            {
                if (resolution.Equals("0x0", StringComparison.OrdinalIgnoreCase))
                {
                    switch (qualityId)
                    {
                        case QUALITY_HIGH:
                            return "1280x720";

                        case QUALITY_MEDIUM:
                            return "852x480";

                        case QUALITY_LOW:
                            return "640x360";

                        case QUALITY_MOBILE:
                            return "284x160";
                    }
                }
                else
                {
                    return resolution;
                }
            }

            return null;
        }

        private int? GetFps(string qualityId, string fps)
        {
            int start = qualityId.IndexOf("p") + 1;

            if (start > 0 && start < qualityId.Length)
            {
                int? qualityFps = decimal.TryParse(qualityId.Substring(start, qualityId.Length - start), out decimal qualityFpsDec) ? (int?)Math.Round(qualityFpsDec, 0) : null;

                if (qualityFps.HasValue)
                {
                    return qualityFps;
                }
            }

            return decimal.TryParse(fps, out decimal fpsDec) ? (int?)Math.Round(fpsDec, 0) : null;
        }

        private int? GetVerticalResolution(string resolution)
        {
            if (string.IsNullOrWhiteSpace(resolution) || !resolution.Contains("x") || resolution.IndexOf("x") >= resolution.Length - 1)
            {
                return null;
            }
            else
            {
                int start = resolution.IndexOf("x") + 1;
                return int.Parse(resolution.Substring(start, resolution.Length - start));
            }
        }

        public int CompareTo(TwitchVideoQuality other)
        {
            if (other == null)
            {
                return -1;
            }

            if (IsSource && !other.IsSource)
            {
                return -1;
            }
            else if (!IsSource && other.IsSource)
            {
                return 1;
            }
            else
            {
                int? thisRes = GetVerticalResolution(Resolution);
                int? otherRes = GetVerticalResolution(other.Resolution);

                if (!thisRes.HasValue && !otherRes.HasValue)
                {
                    return 0;
                }
                else if (!thisRes.HasValue && otherRes.HasValue)
                {
                    return 1;
                }
                else if (thisRes.HasValue && !otherRes.HasValue)
                {
                    return -1;
                }
                else
                {
                    if (thisRes.Value == otherRes.Value)
                    {
                        int? thisFps = Fps;
                        int? otherFps = other.Fps;

                        if (!thisFps.HasValue && !otherFps.HasValue)
                        {
                            return 0;
                        }
                        else if (!thisFps.HasValue && otherFps.HasValue)
                        {
                            return 1;
                        }
                        else if (thisFps.HasValue && !otherFps.HasValue)
                        {
                            return -1;
                        }
                        else
                        {
                            return thisFps > otherFps ? -1 : 1;
                        }
                    }
                    else
                    {
                        return thisRes > otherRes ? -1 : 1;
                    }
                }
            }
        }

        public override string ToString()
        {
            return DisplayString;
        }

        #endregion Methods
    }
}