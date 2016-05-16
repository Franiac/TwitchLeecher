using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TwitchLeecher.Services.Models
{
    public class WebChunkList
    {
        #region Constructors

        public WebChunkList(List<string> header, List<WebChunk> content, List<string> footer)
        {
            if (header == null || header.Count == 0)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (content == null || content.Count == 0)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (footer == null || footer.Count == 0)
            {
                throw new ArgumentNullException(nameof(footer));
            }

            this.Header = header;
            this.Content = content;
            this.Footer = footer;
        }

        #endregion Constructors

        #region Properties

        public List<string> Header { get; private set; }

        public List<WebChunk> Content { get; private set; }

        public List<string> Footer { get; private set; }

        #endregion Properties

        #region Methods

        public static WebChunkList Parse(string tempDir, string playlistStr, string playlistUrlPrefix)
        {
            List<string> header = new List<string>();
            List<WebChunk> content = new List<WebChunk>();
            List<string> footer = new List<string>();

            List<string> lines = playlistStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            int contentIndex = lines.FindIndex(l => l.StartsWith("#EXTINF", StringComparison.OrdinalIgnoreCase));
            int footerIndex = lines.FindLastIndex(l => l.StartsWith("#EXTINF", StringComparison.OrdinalIgnoreCase)) + 2;

            int chunkCounter = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                if (line.StartsWith("#EXT-X-TWITCH", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (i < contentIndex && line.StartsWith("#"))
                {
                    header.Add(line);
                }
                else if (i < footerIndex && line.StartsWith("#EXTINF", StringComparison.OrdinalIgnoreCase))
                {
                    content.Add(new WebChunk(playlistUrlPrefix + lines[i + 1], line, Path.Combine(tempDir, chunkCounter.ToString("D8") + ".ts")));
                    chunkCounter++;
                    i++;
                }
                else if (line.StartsWith("#"))
                {
                    footer.Add(line);
                }
            }

            return new WebChunkList(header, content, footer);
        }

        #endregion Methods
    }
}