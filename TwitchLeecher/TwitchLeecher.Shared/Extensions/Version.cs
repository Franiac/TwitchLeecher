using System;

namespace TwitchLeecher.Shared.Extensions
{
    public static class VersionExtensions
    {
        #region Methods

        public static Version Pad(this Version version)
        {
            if (version.Build < 0 && version.Revision < 0)
            {
                return Version.Parse(version.ToString() + ".0.0");
            }
            else if (version.Revision < 0)
            {
                return Version.Parse(version.ToString() + ".0");
            }
            else
            {
                return version;
            }
        }

        public static Version Trim(this Version version)
        {
            if (version.Build > 0 && version.Revision > 0)
            {
                return version;
            }
            else if (version.Build > 0)
            {
                return Version.Parse(version.ToString(3));
            }
            else
            {
                return Version.Parse(version.ToString(2));
            }
        }

        #endregion Methods
    }
}