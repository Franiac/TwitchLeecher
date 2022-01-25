using System;

namespace TwitchLeecher.Core.Models
{
    public class DefaultQuality
    {
        #region Constructors

        public DefaultQuality(int resolution, string displayString, bool isSource)
        {
            if (string.IsNullOrWhiteSpace(displayString))
            {
                throw new ArgumentNullException(nameof(displayString));
            }

            VerticalResolution = resolution;
            DisplayString = displayString;
            IsSource = isSource;
        }

        #endregion Constructors

        #region Properties

        public int VerticalResolution { get; }

        public string DisplayString { get; }

        public bool IsSource { get; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return DisplayString;
        }

        #endregion Methods
    }
}