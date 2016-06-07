using System;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Services.Extensions
{
    public static class VideoQualityExtensions
    {
        public static string ToTwitchQuality(this VideoQuality videoQuality)
        {
            switch (videoQuality)
            {
                case VideoQuality.Source:
                    return "chunked";

                case VideoQuality.High:
                    return "high";

                case VideoQuality.Medium:
                    return "medium";

                case VideoQuality.Low:
                    return "low";

                case VideoQuality.Mobile:
                    return "mobile";

                case VideoQuality.AudioOnly:
                    return "audio_only";

                default:
                    throw new ApplicationException("Cannot convert enum value '" + videoQuality.ToString() + "' to Twitch quality string!");
            }
        }
    }
}