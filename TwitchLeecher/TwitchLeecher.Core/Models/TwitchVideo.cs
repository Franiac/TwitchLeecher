using System;
using System.Collections.Generic;
using System.Linq;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideo
    {
        #region Constatnts

        private const string UNKNOWN_GAME = "Unknown";

        #endregion Constatnts

        #region Fields

        private string title;
        private string id;
        private string game;

        private int views;

        private TimeSpan length;

        private List<TwitchVideoResolution> resolutions;

        private DateTime recordedDate;

        private Uri thumbnail;
        private Uri url;

        #endregion Fields

        #region Constructors

        public TwitchVideo(string title, string id, string game, int views, TimeSpan length,
            List<TwitchVideoResolution> resolutions, DateTime recordedDate, Uri thumbnail, Uri url)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (resolutions == null || resolutions.Count == 0)
            {
                throw new ArgumentNullException(nameof(resolutions));
            }

            if (thumbnail == null)
            {
                throw new ArgumentNullException(nameof(thumbnail));
            }

            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            this.title = title;
            this.id = id;

            if (string.IsNullOrWhiteSpace(game))
            {
                this.game = UNKNOWN_GAME;
            }
            else
            {
                this.game = game;
            }
            
            this.views = views;
            this.length = length;
            this.resolutions = resolutions;
            this.recordedDate = recordedDate;
            this.thumbnail = thumbnail;
            this.url = url;
        }

        #endregion Constructors

        #region Properties

        public string Title
        {
            get
            {
                return this.title;
            }
        }

        public string Id
        {
            get
            {
                return this.id;
            }
        }

        public string Game
        {
            get
            {
                return this.game;
            }
        }

        public TimeSpan Length
        {
            get
            {
                return this.length;
            }
        }

        public int Views
        {
            get
            {
                return this.views;
            }
        }

        public List<TwitchVideoResolution> Resolutions
        {
            get
            {
                return this.resolutions;
            }
        }

        public string BestResolutionFps
        {
            get
            {
                if (this.resolutions == null || this.resolutions.Count == 0)
                {
                    return "N/A";
                }

                return this.resolutions.First().ResolutionFps;
            }
        }

        public DateTime RecordedDate
        {
            get
            {
                return this.recordedDate;
            }
        }

        public Uri Thumbnail
        {
            get
            {
                return this.thumbnail;
            }
        }

        public Uri Url
        {
            get
            {
                return this.url;
            }
        }

        #endregion Properties
    }
}