using System;
using System.Collections.Generic;
using System.Linq;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideo
    {
        #region Constatnts

        private const string UNTITLED_BROADCAST = "Untitled Broadcast";
        private const string UNKNOWN_GAME = "Unknown";

        #endregion Constatnts

        #region Fields

        private string _channel;
        private string _title;
        private string _id;
        private string _game;

        private int _views;

        private TimeSpan _length;

        private List<TwitchVideoQuality> _resolutions;

        private DateTime _recordedDate;

        private Uri _thumbnail;
        private Uri _url;

        #endregion Fields

        #region Constructors

        public TwitchVideo(string channel, string title, string id, string game, int views, TimeSpan length,
            List<TwitchVideoQuality> resolutions, DateTime recordedDate, Uri thumbnail, Uri url)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentNullException(nameof(channel));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (resolutions == null || resolutions.Count == 0)
            {
                throw new ArgumentNullException(nameof(resolutions));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = UNTITLED_BROADCAST;
            }

            _channel = channel;
            _title = title;
            _id = id;

            if (string.IsNullOrWhiteSpace(game))
            {
                _game = UNKNOWN_GAME;
            }
            else
            {
                _game = game;
            }

            _views = views;
            _length = length;
            _resolutions = resolutions;
            _recordedDate = recordedDate;
            _thumbnail = thumbnail ?? throw new ArgumentNullException(nameof(thumbnail));
            _url = url ?? throw new ArgumentNullException(nameof(url));
        }

        #endregion Constructors

        #region Properties

        public string Channel
        {
            get
            {
                return _channel;
            }
        }

        public string Title
        {
            get
            {
                return _title;
            }
        }

        public string Id
        {
            get
            {
                return _id;
            }
        }

        public string IdTrimmed
        {
            get
            {
                return _id.Substring(1);
            }
        }

        public string Game
        {
            get
            {
                return _game;
            }
        }

        public TimeSpan Length
        {
            get
            {
                return _length;
            }
        }

        public int Views
        {
            get
            {
                return _views;
            }
        }

        public List<TwitchVideoQuality> Resolutions
        {
            get
            {
                return _resolutions;
            }
        }

        public string BestResolutionFps
        {
            get
            {
                if (_resolutions == null || _resolutions.Count == 0)
                {
                    return TwitchVideoQuality.UNKNOWN;
                }

                return _resolutions.First().DisplayStringShort;
            }
        }

        public DateTime RecordedDate
        {
            get
            {
                return _recordedDate;
            }
        }

        public Uri Thumbnail
        {
            get
            {
                return _thumbnail;
            }
        }

        public Uri Url
        {
            get
            {
                return _url;
            }
        }

        #endregion Properties
    }
}