using System;
using System.Collections.Generic;
using System.Linq;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideo : BindableBase
    {
        #region Constatnts

        private const string UNTITLED_BROADCAST = "Untitled Broadcast";
        private const string UNKNOWN_GAME = "Unknown";

        #endregion Constatnts

        #region GlobalMethods

        public static List<Tuple<TimeSpan?, TimeSpan?>> GetListOfSplitTimes(TimeSpan videoLength, TimeSpan? startCrop, TimeSpan? endCrop, TimeSpan splitTime, int overlapSec)
        {
            endCrop = endCrop ?? videoLength;
            List<Tuple<TimeSpan?, TimeSpan?>> result = new List<Tuple<TimeSpan?, TimeSpan?>>();

            TimeSpan? curStart = startCrop;// ?? TimeSpan.Zero;
            TimeSpan curEnd = (curStart ?? TimeSpan.Zero).Add(splitTime.Add(new TimeSpan(0, 0, overlapSec)));
            while (curEnd < endCrop.Value)
            {
                result.Add(new Tuple<TimeSpan?, TimeSpan?>(curStart, curEnd));
                curStart = curEnd.Add(new TimeSpan(0, 0, -overlapSec));
                curEnd = curEnd.Add(splitTime);
            }
            if (endCrop.Value.TotalSeconds - (curStart?.TotalSeconds ?? 0) < Preferences.MinSplitLength && result.Count > 0)
            {//Add remaining seconds to last part
                result[result.Count - 1] = new Tuple<TimeSpan?, TimeSpan?>(result[result.Count - 1].Item1, endCrop == videoLength ? null : endCrop);
            }
            else//or add new last part
                result.Add(new Tuple<TimeSpan?, TimeSpan?>(curStart, endCrop == videoLength ? null : endCrop));
            return result;
        }

        #endregion GlobalMethods

        #region Fields

        private TimeSpan _length;

        #endregion Fields

        #region Constructors

        public TwitchVideo(string channel, string title, string id, string game, int views, TimeSpan length,
            List<TwitchVideoQuality> qualities, DateTime recordedDate, Uri thumbnail, Uri gameThumbnail, Uri url)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentNullException(nameof(channel));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (qualities == null || qualities.Count == 0)
            {
                throw new ArgumentNullException(nameof(qualities));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = UNTITLED_BROADCAST;
            }

            Channel = channel;
            Title = title;
            Id = id;

            if (string.IsNullOrWhiteSpace(game))
            {
                Game = UNKNOWN_GAME;
            }
            else
            {
                Game = game;
            }

            Views = views;
            _length = length;
            Qualities = qualities;
            RecordedDate = recordedDate;
            Thumbnail = thumbnail ?? throw new ArgumentNullException(nameof(thumbnail));
            GameThumbnail = gameThumbnail ?? throw new ArgumentNullException(nameof(gameThumbnail));
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        #endregion Constructors

        #region Properties

        public string Channel { get; }

        public string Title { get; }

        public string Id { get; }

        public string Game { get; }

        public TimeSpan Length
        {
            get
            {
                return _length;
            }
            set
            {
                SetProperty(ref _length, value, nameof(Length));
                FirePropertyChanged(nameof(LengthStr));
            }
        }

        public string LengthStr
        {
            get
            {
                return Length.ToDaylessString();
            }
        }

        public int Views { get; }

        public List<TwitchVideoQuality> Qualities { get; }

        public string BestQuality
        {
            get
            {
                if (Qualities == null || Qualities.Count == 0)
                {
                    return TwitchVideoQuality.UNKNOWN;
                }

                return Qualities.First().ResFpsString;
            }
        }

        public DateTime RecordedDate { get; }

        public Uri Thumbnail { get; }

        public Uri GameThumbnail { get; }

        public Uri Url { get; }

        #endregion Properties
    }
}