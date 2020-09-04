using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TwitchLeecher.Core.Models
{
    public class VodPlaylist : List<VodPlaylistPart>
    {
        #region Static Methods

        public static VodPlaylist Parse(string tempDir, string playlistStr, string urlPrefix)
        {
            VodPlaylist playlist = new VodPlaylist();

            List<string> lines = playlistStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                if (line.StartsWith("#EXT-X-VERSION", StringComparison.OrdinalIgnoreCase))
                {
                    int version = int.Parse(line.Substring(line.LastIndexOf(":") + 1).TrimEnd(), NumberStyles.Integer, CultureInfo.InvariantCulture);

                    if (version == 4)
                    {
                        return ParseV4(tempDir, lines, urlPrefix);
                    }
                    else
                    {
                        return ParseV3(tempDir, lines, urlPrefix);
                    }
                }
            }

            return playlist;
        }

        private static VodPlaylist ParseV3(string tempDir, List<string> lines, string urlPrefix)
        {
            VodPlaylist playlist = new VodPlaylist();

            int partCounter = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                if (line.StartsWith("#EXTINF", StringComparison.OrdinalIgnoreCase))
                {
                    double length = Math.Max(double.Parse(line.Substring(line.LastIndexOf(":") + 1).TrimEnd(','), NumberStyles.Any, CultureInfo.InvariantCulture), 0);

                    playlist.Add(new VodPlaylistPart(length, urlPrefix + lines[i + 1], Path.Combine(tempDir, partCounter.ToString("D8") + ".ts")));
                    partCounter++;
                    i++;
                }
            }

            return playlist;
        }

        private static VodPlaylist ParseV4(string tempDir, List<string> lines, string urlPrefix)
        {
            VodPlaylist playlist = new VodPlaylist();

            int partCounter = 0;
            double lengthBuffer = 0;
            string currentPartStr = null;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                if (line.StartsWith("#EXTINF", StringComparison.OrdinalIgnoreCase))
                {
                    string partStr = lines[i + 2];

                    if (string.IsNullOrWhiteSpace(currentPartStr))
                    {
                        currentPartStr = partStr;
                    }

                    if (!currentPartStr.Equals(partStr))
                    {
                        playlist.Add(new VodPlaylistPart(lengthBuffer, urlPrefix + currentPartStr, Path.Combine(tempDir, partCounter.ToString("D8") + ".ts")));
                        currentPartStr = partStr;
                        lengthBuffer = 0;
                        partCounter++;
                    }

                    lengthBuffer += Math.Max(double.Parse(line.Substring(line.LastIndexOf(":") + 1).TrimEnd(','), NumberStyles.Any, CultureInfo.InvariantCulture), 0);

                    i++;
                }
            }

            if (!string.IsNullOrWhiteSpace(currentPartStr) && lengthBuffer > 0)
            {
                playlist.Add(new VodPlaylistPart(lengthBuffer, urlPrefix + currentPartStr, Path.Combine(tempDir, partCounter.ToString("D8") + ".ts")));
            }

            return playlist;
        }

        #endregion Static Methods
    }
}