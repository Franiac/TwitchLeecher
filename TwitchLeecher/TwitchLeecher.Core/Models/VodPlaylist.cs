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

        #endregion Static Methods
    }
}