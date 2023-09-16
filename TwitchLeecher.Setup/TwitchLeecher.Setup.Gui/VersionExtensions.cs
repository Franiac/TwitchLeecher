using System;

namespace TwitchLeecher.Setup.Gui
{
    public static class VersionExtensions
    {
        public static Version Pad(this Version version)
        {
            if (version.Build < 0 && version.Revision < 0)
            {
                return Version.Parse(version + ".0.0");
            }

            if (version.Revision < 0)
            {
                return Version.Parse(version + ".0");
            }

            return version;
        }

        public static Version Trim(this Version version)
        {
            if (version.Build > 0 && version.Revision > 0)
            {
                return version;
            }

            if (version.Build > 0)
            {
                return Version.Parse(version.ToString(3));
            }

            return Version.Parse(version.ToString(2));
        }
    }
}