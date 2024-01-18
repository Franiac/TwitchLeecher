using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using TwitchLeecher.Shared.Extensions;

namespace TwitchLeecher.Core.Models
{
    public class TwitchVideo
    {
        #region Constatnts

        private const string UNTITLED_BROADCAST = "Untitled Broadcast";

        #endregion Constatnts

        #region Constructors

        public TwitchVideo(string channel, string title, string id, int views, TimeSpan length, DateTime recordedDate, Uri thumbnail, Uri url, bool viewable, bool muted, bool live)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentNullException(nameof(channel));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                title = UNTITLED_BROADCAST;
            }

            Channel = channel;
            Title = title;
            Id = id;
            Views = views;
            Length = length;
            RecordedDate = recordedDate;
            Thumbnail = ImageHelper.LoadFromWeb(thumbnail);
            Url = url;
            Muted = muted;
            Live = live;
        }

        #endregion Constructors

        #region Properties

        public string Channel { get; }

        public string Title { get; }

        public string Id { get; }

        public TimeSpan Length { get; }

        public string LengthStr
        {
            get
            {
                return Length.ToDaylessString();
            }
        }

        public int Views { get; }

        public DateTime RecordedDate { get; }

        public Task<Bitmap> Thumbnail { get; }

        public Uri Url { get; }

        public bool Viewable { get; }

        public bool Muted { get; }

        public bool Live { get; }

        public TimeSpan? StartTime { get; set; }

        #endregion Properties
    }
}