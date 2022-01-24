using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using TwitchLeecher.Core;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;

namespace TwitchLeecher.Services.Services
{
    internal class ApiService : IApiService
    {
        #region Constants

        private const string USERS_URL = "https://api.twitch.tv/helix/users";
        private const string CHANNEL_URL = "https://api.twitch.tv/helix/channels";
        private const string VIDEOS_URL = "https://api.twitch.tv/helix/videos";
        private const string GAMES_URL = "https://api.twitch.tv/kraken/games/top";
        private const string ACCESS_TOKEN_URL = "https://gql.twitch.tv/gql";
        private const string ALL_PLAYLISTS_URL = "https://usher.ttvnw.net/vod/{0}.m3u8?nauthsig={1}&nauth={2}&allow_source=true&player=twitchweb&allow_spectre=true&allow_audio_only=true";
        private const string UNKNOWN_GAME_URL = "https://static-cdn.jtvnw.net/ttv-boxart/404_boxart.png";

        private const int TWITCH_MAX_LOAD_LIMIT = 100;

        #endregion Constants

        #region Fields

        private readonly IRuntimeDataService _runtimeDataService;

        #endregion Fields

        #region Constructors

        public ApiService(IRuntimeDataService runtimeDataService)
        {
            _runtimeDataService = runtimeDataService;
        }

        #endregion Constructors

        #region Methods

        private WebClient CreateApiWebClient()
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("Client-Id", Constants.ClientId);
            wc.Headers.Add("Authorization", $"Bearer { _runtimeDataService.RuntimeData.AccessToken }");

            return wc;
        }

        public TwitchAuthInfo ValidateAuthentication(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return null;
            }

            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add(HttpRequestHeader.Authorization, $"Bearer { accessToken }");

                string jsonStr = null;

                try
                {
                    jsonStr = wc.DownloadString("https://id.twitch.tv/oauth2/validate");
                }
                catch (WebException)
                {
                    // Any WebException indicates that the access token could not be verified
                    return null;
                }

                if (!string.IsNullOrWhiteSpace(jsonStr))
                {
                    JObject json = JObject.Parse(jsonStr);

                    if (json != null)
                    {
                        string login = json.Value<string>("login");
                        string userId = json.Value<string>("user_id");
                        string clientId = json.Value<string>("client_id");

                        if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(clientId) && clientId.Equals(Constants.ClientId, StringComparison.OrdinalIgnoreCase))
                        {
                            return new TwitchAuthInfo(accessToken, login, userId);
                        }
                    }
                }
            }

            return null;
        }

        public void RevokeAuthentication(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return;
            }

            using (WebClient wc = new WebClient())
            {
                try
                {
                    _ = wc.UploadString($"https://id.twitch.tv/oauth2/revoke?client_id={ Constants.ClientId }&token={ accessToken }", string.Empty);
                }
                catch (WebException)
                {
                    // Ignore potentionally failed revoke requests
                }
            }
        }

        public bool ChannelExists(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentNullException(nameof(channel));
            }

            return GetChannelIdByName(channel) != null;
        }

        public string GetChannelIdByName(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentNullException(nameof(channel));
            }

            using (WebClient webClient = CreateApiWebClient())
            {
                webClient.QueryString.Add("login", channel);

                string result = null;

                try
                {
                    result = webClient.DownloadString(USERS_URL);
                }
                catch (WebException)
                {
                    return null;
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    JObject searchResultJson = JObject.Parse(result);

                    JArray usersJson = searchResultJson.Value<JArray>("data");

                    if (usersJson != null && usersJson.HasValues)
                    {
                        JToken userJson = usersJson.FirstOrDefault();

                        if (userJson != null)
                        {
                            string id = userJson.Value<string>("id");

                            if (!string.IsNullOrWhiteSpace(id))
                            {
                                using (WebClient webClientChannel = CreateApiWebClient())
                                {
                                    webClientChannel.QueryString.Add("broadcaster_id", id);

                                    try
                                    {
                                        _ = webClientChannel.DownloadString(CHANNEL_URL);
                                        return id;
                                    }
                                    catch (WebException)
                                    {
                                        return null;
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                }

                return null;
            }
        }

        public ObservableCollection<TwitchVideo> Search(SearchParameters searchParams)
        {
            if (searchParams == null)
            {
                throw new ArgumentNullException(nameof(searchParams));
            }

            SearchType searchType = searchParams.SearchType;

            if (searchType == SearchType.Channel)
            {
                return SearchChannel(searchParams.Channel, searchParams.VideoType, searchParams.LoadLimitType, searchParams.LoadFrom.Value, searchParams.LoadTo.Value, searchParams.LoadLastVods);
            }
            else if (searchType == SearchType.Urls)
            {
                return SearchUrls(searchParams.Urls);
            }
            else
            {
                return SearchIds(searchParams.Ids);
            }
        }

        private ObservableCollection<TwitchVideo> SearchChannel(string channel, VideoType videoType, LoadLimitType loadLimit, DateTime loadFrom, DateTime loadTo, int loadLastVods)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentNullException(nameof(channel));
            }

            string channelId = GetChannelIdByName(channel);

            ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

            string broadcastTypeParam;

            if (videoType == VideoType.Broadcast)
            {
                broadcastTypeParam = "archive";
            }
            else if (videoType == VideoType.Highlight)
            {
                broadcastTypeParam = "highlight";
            }
            else if (videoType == VideoType.Upload)
            {
                broadcastTypeParam = "upload";
            }
            else
            {
                throw new ApplicationException("Unsupported video type '" + videoType.ToString() + "'");
            }

            DateTime fromDate = DateTime.Now;
            DateTime toDate = DateTime.Now;

            if (loadLimit == LoadLimitType.Timespan)
            {
                fromDate = loadFrom;
                toDate = loadTo;
            }

            int sum = 0;
            bool stop = false;
            string cursor = null;

            do
            {
                using (WebClient webClient = CreateApiWebClient())
                {
                    webClient.QueryString.Add("user_id", channelId);
                    webClient.QueryString.Add("type", broadcastTypeParam);
                    webClient.QueryString.Add("first", TWITCH_MAX_LOAD_LIMIT.ToString());

                    if (!string.IsNullOrWhiteSpace(cursor))
                    {
                        webClient.QueryString.Add("after", cursor);
                    }

                    string result = webClient.DownloadString(VIDEOS_URL);

                    JObject videosResponseJson = JObject.Parse(result);

                    if (videosResponseJson != null)
                    {
                        JArray videosJson = videosResponseJson.Value<JArray>("data");

                        if (videosJson.Count == 0)
                        {
                            stop = true;
                        }
                        else
                        {
                            JObject paginationJson = videosResponseJson.Value<JObject>("pagination");

                            if (paginationJson.ContainsKey("cursor"))
                            {
                                cursor = paginationJson.Value<string>("cursor");
                            }

                            foreach (JObject videoJson in videosJson)
                            {
                                sum++;

                                TwitchVideo video = ParseVideo(videoJson);

                                if (loadLimit == LoadLimitType.LastVods)
                                {
                                    videos.Add(video);

                                    if (sum >= loadLastVods)
                                    {
                                        stop = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    DateTime recordedDate = video.RecordedDate;

                                    if (recordedDate.Date >= fromDate.Date && recordedDate.Date <= toDate.Date)
                                    {
                                        videos.Add(video);
                                    }

                                    if (recordedDate.Date < fromDate.Date)
                                    {
                                        stop = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            } while (!stop);

            return videos;
        }

        private ObservableCollection<TwitchVideo> SearchUrls(string urls)
        {
            if (string.IsNullOrWhiteSpace(urls))
            {
                throw new ArgumentNullException(nameof(urls));
            }

            ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

            string[] urlArr = urls.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            if (urlArr.Length > 0)
            {
                HashSet<string> addedIds = new HashSet<string>();

                foreach (string url in urlArr)
                {
                    string id = GetVideoIdFromUrl(url);

                    if (!string.IsNullOrWhiteSpace(id) && !addedIds.Contains(id))
                    {
                        TwitchVideo video = GetTwitchVideoFromId(id);

                        if (video != null)
                        {
                            videos.Add(video);
                            addedIds.Add(id);
                        }
                    }
                }
            }

            return videos;
        }

        private ObservableCollection<TwitchVideo> SearchIds(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                throw new ArgumentNullException(nameof(ids));
            }

            ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

            string[] idsArr = ids.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            if (idsArr.Length > 0)
            {
                HashSet<string> addedIds = new HashSet<string>();

                foreach (string id in idsArr)
                {
                    if (!string.IsNullOrWhiteSpace(id) && !addedIds.Contains(id))
                    {
                        TwitchVideo video = GetTwitchVideoFromId(id);

                        if (video != null)
                        {
                            videos.Add(video);
                            addedIds.Add(id);
                        }
                    }
                }
            }

            return videos;
        }

        public VodAuthInfo RetrieveVodAuthInfo(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            using (WebClient webClient = new WebClient())
            {
                string accessTokenStr = webClient.UploadString(ACCESS_TOKEN_URL, CreateGqlPlaybackAccessToken(id));

                JObject accessTokenJson = JObject.Parse(accessTokenStr);

                JToken vpaToken = accessTokenJson.SelectToken("$.data.videoPlaybackAccessToken", false);

                string token = Uri.EscapeDataString(vpaToken.Value<string>("value"));
                string signature = vpaToken.Value<string>("signature");

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new ApplicationException("VOD access token is null!");
                }

                if (string.IsNullOrWhiteSpace(signature))
                {
                    throw new ApplicationException("VOD signature is null!");
                }

                bool privileged = false;
                bool subOnly = false;

                JObject tokenJson = JObject.Parse(HttpUtility.UrlDecode(token));

                if (tokenJson == null)
                {
                    throw new ApplicationException("Decoded VOD access token is null!");
                }

                privileged = tokenJson.Value<bool>("privileged");

                if (privileged)
                {
                    subOnly = true;
                }
                else
                {
                    JObject chansubJson = tokenJson.Value<JObject>("chansub");

                    if (chansubJson == null)
                    {
                        throw new ApplicationException("Token property 'chansub' is null!");
                    }

                    JArray restrictedQualitiesJson = chansubJson.Value<JArray>("restricted_bitrates");

                    if (restrictedQualitiesJson == null)
                    {
                        throw new ApplicationException("Token property 'chansub -> restricted_bitrates' is null!");
                    }

                    if (restrictedQualitiesJson.Count > 0)
                    {
                        subOnly = true;
                    }
                }

                return new VodAuthInfo(token, signature, privileged, subOnly);
            }
        }

        private string CreateGqlPlaybackAccessToken(string id)
        {
            // {
            //   "operationName": "PlaybackAccessToken",
            //   "variables": {
            //       "isLive": false,
            //       "login": "",
            //       "isVod": true,
            //       "vodID": "870835569",
            //       "playerType": "channel_home_live"
            //   },
            //   "extensions": {
            //     "persistedQuery": {
            //       "version": 1,
            //       "sha256Hash": "0828119ded1c13477966434e15800ff57ddacf13ba1911c129dc2200705b0712"
            //     }
            //   }
            // }

            return "{\"operationName\": \"PlaybackAccessToken\",\"variables\": {\"isLive\": false,\"login\": \"\",\"isVod\": true,\"vodID\": \"" + id + "\",\"playerType\": \"channel_home_live\"},\"extensions\": {\"persistedQuery\": {\"version\": 1,\"sha256Hash\": \"0828119ded1c13477966434e15800ff57ddacf13ba1911c129dc2200705b0712\"}}}";
        }

        private string RetrievePlaylistUrlForQuality(Action<string> log, TwitchVideoQuality quality, string vodId, VodAuthInfo vodAuthInfo)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("Accept", "*/*");
                webClient.Headers.Add("Accept-Encoding", "gzip, deflate, br");

                log(Environment.NewLine + Environment.NewLine + "Retrieving m3u8 playlist urls for all VOD qualities...");
                string allPlaylistsStr = webClient.DownloadString(string.Format(ALL_PLAYLISTS_URL, vodId, vodAuthInfo.Signature, vodAuthInfo.Token));
                log(" done!");

                List<string> allPlaylistsList = allPlaylistsStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(s => !s.StartsWith("#")).ToList();

                allPlaylistsList.ForEach(url =>
                {
                    log(Environment.NewLine + url);
                });

                string playlistUrl = allPlaylistsList.Where(s => s.ToLowerInvariant().Contains("/" + quality.QualityId + "/")).First();

                log(Environment.NewLine + Environment.NewLine + "Playlist url for selected quality " + quality.DisplayString + " is " + playlistUrl);

                return playlistUrl;
            }
        }

        private VodPlaylist RetrieveVodPlaylist(Action<string> log, string tempDir, string playlistUrl)
        {
            using (WebClient webClient = new WebClient())
            {
                log(Environment.NewLine + Environment.NewLine + "Retrieving playlist...");
                string playlistStr = webClient.DownloadString(playlistUrl);
                log(" done!");

                if (string.IsNullOrWhiteSpace(playlistStr))
                {
                    throw new ApplicationException("The playlist is empty!");
                }

                string urlPrefix = playlistUrl.Substring(0, playlistUrl.LastIndexOf("/") + 1);

                log(Environment.NewLine + "Parsing playlist...");
                VodPlaylist vodPlaylist = VodPlaylist.Parse(tempDir, playlistStr, urlPrefix);
                log(" done!");

                log(Environment.NewLine + "Number of video chunks: " + vodPlaylist.Count());

                return vodPlaylist;
            }
        }

        private string GetVideoIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri validUrl))
            {
                return null;
            }

            string[] segments = validUrl.Segments;

            if (segments.Length < 2)
            {
                return null;
            }

            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].Equals("video/", StringComparison.OrdinalIgnoreCase) || segments[i].Equals("videos/", StringComparison.OrdinalIgnoreCase))
                {
                    if (segments.Length > (i + 1))
                    {
                        string idStr = segments[i + 1];

                        if (!string.IsNullOrWhiteSpace(idStr))
                        {
                            idStr = idStr.Trim(new char[] { '/' });

                            if (!string.IsNullOrWhiteSpace(idStr))
                            {
                                return idStr;
                            }
                        }
                    }

                    break;
                }
            }

            return null;
        }

        private TwitchVideo GetTwitchVideoFromId(string id)
        {
            using (WebClient webClient = CreateApiWebClient())
            {
                try
                {
                    webClient.QueryString.Add("id", id);

                    string result = webClient.DownloadString(VIDEOS_URL);

                    JObject videosResponseJson = JObject.Parse(result);

                    if (videosResponseJson != null)
                    {
                        JArray videosJson = videosResponseJson.Value<JArray>("data");

                        if (videosJson != null)
                        {
                            JToken videoJson = videosJson.FirstOrDefault();

                            if (videoJson != null)
                            {
                                return ParseVideo(videoJson);
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse resp && resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return null;
        }

        public TwitchVideo ParseVideo(JToken videoJson)
        {
            string id = videoJson.Value<string>("id");
            string title = videoJson.Value<string>("title");
            string channel = videoJson.Value<string>("user_name");
            int views = videoJson.Value<int>("view_count");
            bool viewable = videoJson.Value<string>("viewable").Equals("public", StringComparison.OrdinalIgnoreCase);
            string url = videoJson.Value<string>("url");
            string thumbnail = videoJson.Value<string>("thumbnail_url");
            TimeSpan length = ParseTwitchDuration(videoJson.Value<string>("duration"));
            DateTime recordedDate = DateTime.Parse(videoJson.Value<string>("published_at"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

            thumbnail = thumbnail.Replace("%{width}", "320").Replace("%{height}", "180");

            return new TwitchVideo(channel, title, id, views, length, recordedDate, new Uri(thumbnail), new Uri(url), viewable);
        }

        private TimeSpan ParseTwitchDuration(string durationStr)
        {
            if (string.IsNullOrWhiteSpace(durationStr))
            {
                throw new ArgumentException("The string to parse is null or empty!", nameof(durationStr));
            }

            int hourIndex = durationStr.IndexOf("h");
            int minIndex = durationStr.IndexOf("m");
            int secIndex = durationStr.IndexOf("s");

            bool hasHour = hourIndex >= 0;
            bool hasMin = minIndex >= 0;
            bool hasSec = secIndex >= 0;

            string hourStr = null;
            string minStr = null;
            string secStr = null;

            if (hasHour)
            {
                hourStr = durationStr.Substring(0, hourIndex);
            }

            if (hasMin)
            {
                minStr = durationStr.Substring(hasHour ? hourIndex + 1 : 0, hasHour ? minIndex - hourIndex - 1 : minIndex);
            }

            if (hasSec)
            {
                secStr = durationStr.Substring(hasMin ? minIndex + 1 : 0, hasMin ? secIndex - minIndex - 1 : secIndex);
            }

            int? hour = null;
            int? min = null;
            int? sec = null;

            if (int.TryParse(hourStr, out int parsedHour))
            {
                hour = parsedHour;
            }

            if (int.TryParse(minStr, out int parsedMin))
            {
                min = parsedMin;
            }

            if (int.TryParse(secStr, out int parsedSec))
            {
                sec = parsedSec;
            }

            if (hour == null && min == null && sec == null)
            {
                throw new ArgumentException($"Cannot parse string '{durationStr}'!", nameof(durationStr));
            }

            return new TimeSpan(hour ?? 0, min ?? 0, sec ?? 0);
        }

        #endregion Methods
    }
}