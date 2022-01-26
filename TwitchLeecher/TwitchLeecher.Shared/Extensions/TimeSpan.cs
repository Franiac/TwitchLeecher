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

        public static TimeSpan ParseTwitchFormat(string durationStr)
        {
            if (string.IsNullOrWhiteSpace(durationStr))
            {
                throw new ArgumentException("The string to parse is null or empty!", nameof(durationStr));
            }

            int hourIndex = durationStr.IndexOf("h");
            int minIndex = durationStr.IndexOf("m");
            int secIndex = durationStr.IndexOf("s");

            bool hasHour = hourIndex >= 0;
            bool hasMin = minIndex >= 0;
            bool hasSec = secIndex >= 0;

            string hourStr = null;
            string minStr = null;
            string secStr = null;

            if (hasHour)
            {
                hourStr = durationStr.Substring(0, hourIndex);
            }

            if (hasMin)
            {
                minStr = durationStr.Substring(hasHour ? hourIndex + 1 : 0, hasHour ? minIndex - hourIndex - 1 : minIndex);
            }

            if (hasSec)
            {
                secStr = durationStr.Substring(hasMin ? minIndex + 1 : 0, hasMin ? secIndex - minIndex - 1 : secIndex);
            }

            int? hour = null;
            int? min = null;
            int? sec = null;

            if (int.TryParse(hourStr, out int parsedHour))
            {
                hour = parsedHour;
            }

            if (int.TryParse(minStr, out int parsedMin))
            {
                min = parsedMin;
            }

            if (int.TryParse(secStr, out int parsedSec))
            {
                sec = parsedSec;
            }

            if (hour == null && min == null && sec == null)
            {
                throw new ArgumentException($"Cannot parse string '{durationStr}'!", nameof(durationStr));
            }

            return new TimeSpan(hour ?? 0, min ?? 0, sec ?? 0);
        }

        public static bool TryParseTwitchFormat(string durationStr, out TimeSpan result)
        {
            try
            {
                result = ParseTwitchFormat(durationStr);
                return true;
            }
            catch
            {
                result = TimeSpan.Zero;
                return false;
            }
        }
    }
}