using System;

namespace TwitchLeecher.Core.Models
{
    public class DefaultQuality
    {
        #region Static Fields

        public static int SOURCE_RES = 1080;
        public static int AUDIO_ONLY_RES = 0;

        #endregion Static Fields

        #region Constructors

        public DefaultQuality(int resolution, string displayString)
        {
            if (string.IsNullOrWhiteSpace(displayString))
            {
                throw new ArgumentNullException(nameof(displayString));
            }

            VerticalResolution = resolution;
            DisplayString = displayString;
            IsSource = resolution == SOURCE_RES;
            IsAudioOnly = resolution == AUDIO_ONLY_RES;
        }

        #endregion Constructors

        #region Properties

        public int VerticalResolution { get; }

        public string DisplayString { get; }

        public bool IsSource { get; }

        public bool IsAudioOnly { get; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return DisplayString;
        }

        #endregion Methods
    }
}