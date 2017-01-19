using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TwitchLeecher.Core.Models
{
    public class VodPlaylist : List<IVodPlaylistPart>
    {
        #region Static Methods

        public static VodPlaylist Parse(string tempDir, string playlistStr, string urlPrefix)
        {
            VodPlaylist playlist = new VodPlaylist();

            List<string> lines = playlistStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            int indexCounter = 0;
            int partCounter = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#EXT-X-TWITCH", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (line.StartsWith("#EXTINF", StringComparison.OrdinalIgnoreCase))
                {
                    playlist.Add(new VodPlaylistPartExt(indexCounter, line, lines[i + 1], urlPrefix, Path.Combine(tempDir, partCounter.ToString("D8") + ".ts")));
                    partCounter++;
                    i++;
                }
                else
                {
                    playlist.Add(new VodPlaylistPart(indexCounter, line));
                }

                indexCounter++;
            }

            if (!playlist.Last().GetOutput().Equals("#EXT-X-ENDLIST", StringComparison.OrdinalIgnoreCase))
            {
                playlist.Add(new VodPlaylistPart(indexCounter, "#EXT-X-ENDLIST"));
            }

            return playlist;
        }

        #endregion Static Methods
    }
}