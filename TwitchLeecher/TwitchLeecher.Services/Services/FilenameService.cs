using System;
using System.Globalization;
using System.IO;
using System.Linq;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Extensions;

namespace TwitchLeecher.Services.Services
{
    internal class FilenameService : IFilenameService
    {
        #region Methods

        public string SubstituteWildcards(string filename, string folder, FilenameWildcards.IsFileNameUsedsDelegate IsFileNameUsed, TwitchVideo video, TwitchVideoQuality quality = null, TimeSpan? cropStart = null, TimeSpan? cropEnd = null)
        {
            if (video == null)
            {
                throw new ArgumentNullException(nameof(video));
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                return filename;
            }

            string result = filename;

            DateTime recorded = video.RecordedDate;

            TwitchVideoQuality selectedQuality = quality ?? video.Qualities.First();
            TimeSpan selectedCropStart = cropStart ?? TimeSpan.Zero;
            TimeSpan selectedCropEnd = cropEnd ?? video.Length;

            result = result.Replace(FilenameWildcards.CHANNEL, video.Channel);
            result = result.Replace(FilenameWildcards.GAME, video.Game);
            result = result.Replace(FilenameWildcards.DATE, recorded.ToString("yyyyMMdd"));
            result = result.Replace(FilenameWildcards.TIME, recorded.ToString("hhmmsstt", CultureInfo.InvariantCulture));
            result = result.Replace(FilenameWildcards.TIME24, recorded.ToString("HHmmss", CultureInfo.InvariantCulture));
            result = result.Replace(FilenameWildcards.DATE_, recorded.ToString("yyyy-MM-dd"));
            result = result.Replace(FilenameWildcards.TIME_, recorded.ToString("hh-mm-ss_tt", CultureInfo.InvariantCulture));
            result = result.Replace(FilenameWildcards.TIME24_, recorded.ToString("HH-mm-ss", CultureInfo.InvariantCulture));
            result = result.Replace(FilenameWildcards.ID, video.Id);
            result = result.Replace(FilenameWildcards.TITLE, video.Title);
            result = result.Replace(FilenameWildcards.RES, !string.IsNullOrWhiteSpace(selectedQuality.Resolution) ? selectedQuality.Resolution : TwitchVideoQuality.UNKNOWN);
            result = result.Replace(FilenameWildcards.FPS, selectedQuality.Fps.HasValue ? selectedQuality.Fps.ToString() : TwitchVideoQuality.UNKNOWN);
            result = result.Replace(FilenameWildcards.START, selectedCropStart.ToShortDaylessString());
            result = result.Replace(FilenameWildcards.END, selectedCropEnd.ToShortDaylessString());

            result = SubstituteInvalidChars(result, "_");

            if (result.Contains(FilenameWildcards.UNIQNUMBER))
            {
                int index = 1;
                while (File.Exists(Path.Combine(folder, result.Replace(FilenameWildcards.UNIQNUMBER, index.ToString()))) || IsFileNameUsed(Path.Combine(folder, result.Replace(FilenameWildcards.UNIQNUMBER, index.ToString()))))
                    index++;
                result = result.Replace(FilenameWildcards.UNIQNUMBER, index.ToString());
            }
            
            return result;
        }

        public string SubstituteInvalidChars(string filename, string replaceStr)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return filename;
            }

            if (string.IsNullOrEmpty(replaceStr))
            {
                throw new ArgumentNullException(nameof(replaceStr));
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c.ToString(), replaceStr);
            }

            return filename;
        }

        public string EnsureExtension(string filename, bool disableConversion)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return filename;
            }

            if (disableConversion && !filename.EndsWith(".ts"))
            {
                return filename + ".ts";
            }
            else if (disableConversion && filename.EndsWith(".mp4"))
            {
                return filename.Substring(0, filename.Length - 4) + ".ts";
            }
            else if (!disableConversion && !filename.EndsWith(".mp4"))
            {
                return filename + ".mp4";
            }
            else if (!disableConversion && filename.EndsWith(".ts"))
            {
                return filename.Substring(0, filename.Length - 3) + ".mp4";
            }

            return filename;
        }

        #endregion Methods
    }
}