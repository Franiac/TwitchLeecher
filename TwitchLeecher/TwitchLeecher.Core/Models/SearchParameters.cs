using System;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class SearchParameters : BindableBase
    {
        #region Fields

        private SearchType searchType;
        private VideoType videoType;

        private string channel;
        private string urls;
        private string ids;

        private int loadLimit;

        #endregion Fields

        #region Constructors

        public SearchParameters(SearchType searchType)
        {
            this.searchType = searchType;
        }

        #endregion Constructors

        #region Properties

        public SearchType SearchType
        {
            get
            {
                return this.searchType;
            }
            set
            {
                this.SetProperty(ref this.searchType, value, nameof(this.SearchType));
            }
        }

        public VideoType VideoType
        {
            get
            {
                return this.videoType;
            }
            set
            {
                this.SetProperty(ref this.videoType, value, nameof(this.VideoType));
            }
        }

        public string Channel
        {
            get
            {
                return this.channel;
            }
            set
            {
                this.SetProperty(ref this.channel, value, nameof(this.Channel));
            }
        }

        public string Urls
        {
            get
            {
                return this.urls;
            }
            set
            {
                this.SetProperty(ref this.urls, value, nameof(this.Urls));
            }
        }

        public string Ids
        {
            get
            {
                return this.ids;
            }
            set
            {
                this.SetProperty(ref this.ids, value, nameof(this.Ids));
            }
        }

        public int LoadLimit
        {
            get
            {
                return this.loadLimit;
            }
            set
            {
                this.SetProperty(ref this.loadLimit, value, nameof(this.LoadLimit));
            }
        }

        #endregion Properties

        #region Methods

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(this.Channel);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (this.searchType == SearchType.Channel && string.IsNullOrWhiteSpace(this.channel))
                {
                    this.AddError(currentProperty, "Please specify a channel name!");
                }
            }

            currentProperty = nameof(this.Urls);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (this.searchType == SearchType.Urls)
                {
                    if (string.IsNullOrWhiteSpace(this.urls))
                    {
                        this.AddError(currentProperty, "Please specify one or more Twitch video urls!");
                    }
                    else
                    {
                        Action addError = () =>
                        {
                            this.AddError(currentProperty, "One or more urls are invalid!");
                        };

                        string[] urls = this.urls.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                        if (urls.Length > 0)
                        {
                            foreach (string url in urls)
                            {
                                Uri validUrl;

                                if (!Uri.TryCreate(url, UriKind.Absolute, out validUrl))
                                {
                                    addError();
                                    break;
                                }

                                string[] segments = validUrl.Segments;

                                if (segments.Length < 2)
                                {
                                    addError();
                                    break;
                                }

                                bool validId = false;

                                for (int i = 0; i < segments.Length; i++)
                                {
                                    if (segments[i].Equals("v/", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (segments.Length > (i + 1))
                                        {
                                            string idStr = segments[i + 1];

                                            if (!string.IsNullOrWhiteSpace(idStr))
                                            {
                                                idStr = idStr.Trim(new char[] { '/' });

                                                int idInt;

                                                if (int.TryParse(idStr, out idInt) && idInt > 0)
                                                {
                                                    validId = true;
                                                    break;
                                                }
                                            }
                                        }

                                        break;
                                    }
                                }

                                if (!validId)
                                {
                                    addError();
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            currentProperty = nameof(this.Ids);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (this.searchType == SearchType.Ids)
                {
                    if (string.IsNullOrWhiteSpace(this.ids))
                    {
                        this.AddError(currentProperty, "Please specify one or more Twitch video IDs!");
                    }
                    else
                    {
                        string[] ids = this.ids.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                        if (ids.Length > 0)
                        {
                            foreach (string id in ids)
                            {
                                string idStr = id.TrimStart(new char[] { 'v' });

                                int idInt;

                                if (!int.TryParse(idStr, out idInt))
                                {
                                    this.AddError(currentProperty, "One or more IDs are invalid!");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public SearchParameters Clone()
        {
            return new SearchParameters(this.searchType)
            {
                VideoType = this.videoType,
                Channel = this.channel,
                Urls = this.urls,
                Ids = this.ids,
                LoadLimit = this.loadLimit
            };
        }

        #endregion Methods
    }
}