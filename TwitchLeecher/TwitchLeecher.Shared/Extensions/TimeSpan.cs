using System;

namespace TwitchLeecher.Shared.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToDaylessString(this TimeSpan value)
        {
            return string.Format("{0}:{1}:{2}", value.GetDaysInHours().ToString("00"), value.Minutes.ToString("00"), value.Seconds.ToString("00"));
        }

        public static string ToShortDaylessString(this TimeSpan value)
        {
            return string.Format("{0}{1}{2}", value.GetDaysInHours().ToString("00"), value.Minutes.ToString("00"), value.Seconds.ToString("00"));
        }

        public static int GetDaysInHours(this TimeSpan value)
        {
            return (value.Days * 24) + value.Hours;
        }
    }
}