using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using TwitchLeecher.Core.Constants;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Extensions;

namespace TwitchLeecher.Services.Services
{
    internal class ApiService : IApiService
    {
        #region Constants

        private const string GQL_URL = "https://gql.twitch.tv/gql";
        private const string AUTH_URL = "https://id.twitch.tv/oauth2/validate";
        private const string REVOKE_URL = "https://id.twitch.tv/oauth2/revoke";
        private const string USERS_URL = "https://api.twitch.tv/helix/users";
        private const string VIDEOS_URL = "https://api.twitch.tv/helix/videos";
        private const string CHANNELS_URL = "https://api.twitch.tv/helix/channels";
        private const string PLAYLISTS_URL = "https://usher.ttvnw.net/vod/{0}.m3u8";

        private const string PROCESSING_THUMBNAIL = "https://vod-secure.twitch.tv/_404/404_processing_320x180.png";

        private const int TWITCH_MAX_LOAD_LIMIT = 100;

        #endregion Constants

        #region Fields

        private readonly IRuntimeDataService _runtimeDataService;

        private readonly Regex _rxGroup = new Regex("GROUP-ID\\=\"(?<group>.*?)\\\"");
        private readonly Regex _rxName = new Regex("NAME\\=\"(?<name>.*?)\\\"");
        private readonly Regex _rxResolution = new Regex("RESOLUTION\\=(?<resolution>.*?),");

        private DateTime? _nextRequest;

        #endregion Fields

        #region Constructors

        public ApiService(IRuntimeDataService runtimeDataService)
        {
            _runtimeDataService = runtimeDataService;
        }

        #endregion Constructors

        #region Methods

        private WebClient CreateWebClientWithEncoding()
        {
            return new WebClient()
            {
                Encoding = Encoding.UTF8
            };
        }

        private WebClient CreateApiWebClient()
        {
            WebClient wc = CreateWebClientWithEncoding();
            wc.Headers.Add("Client-ID", Constants.ClientId);
            wc.Headers.Add("Authorization", $"Bearer { _runtimeDataService.RuntimeData.AuthInfo.AccessToken }");

            return wc;
        }

        private WebClient CreateGqlWebClient()
        {
            WebClient wc = CreateWebClientWithEncoding();
            wc.Headers.Add("Client-ID", Constants.ClientIdWeb);

            string accessTokenSubOnly = _runtimeDataService.RuntimeData.AuthInfo.AccessTokenSubOnly;

            if (!string.IsNullOrWhiteSpace(accessTokenSubOnly))
            {
                wc.Headers.Add("Authorization", $"OAuth { accessTokenSubOnly }");
            }

            return wc;
        }

        private void SetNextRequestTime(WebClient wc)
        {
            string resetStr = wc.ResponseHeaders["Ratelimit-Reset"];

            if (!string.IsNullOrWhiteSpace(resetStr) && long.TryParse(resetStr, out long reset))
            {
                _nextRequest = DateTimeOffset.FromUnixTimeSeconds(reset).DateTime;
            }
        }

        private void WaitForNextRequest()
        {
            if (_nextRequest == null)
            {
                return;
            }

            DateTime utcNow = DateTime.UtcNow;

            if (utcNow < _nextRequest.Value)
            {
                Thread.Sleep((int)Math.Ceiling((_nextRequest.Value - utcNow).TotalMilliseconds));
            }
        }

        public bool ValidateAuthentication(string accessToken, bool subOnly)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return false;
            }

            using (WebClient webClient = CreateWebClientWithEncoding())
            {
                webClient.Headers.Add("Authorization", $"Bearer { accessToken }");

                string jsonStr = null;

                try
                {
                    jsonStr = webClient.DownloadString(AUTH_URL);
                }
                catch (WebException)
                {
                    // Any WebException indicates that the access token could not be verified
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(jsonStr))
                {
                    JObject json = JObject.Parse(jsonStr);

                    if (json != null)
                    {
                        string login = json.Value<string>("login");
                        string userId = json.Value<string>("user_id");
                        string clientId = json.Value<string>("client_id");

                        string checkClientId = subOnly ? Constants.ClientIdWeb : Constants.ClientId;

                        if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(clientId) && clientId.Equals(checkClientId, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void RevokeAuthentication(string accessToken, bool subOnly)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return;
            }

            using (WebClient webClient = CreateWebClientWithEncoding())
            {
                webClient.QueryString.Add("client_id", subOnly ? Constants.ClientIdWeb : Constants.ClientId);
                webClient.QueryString.Add("token", accessToken);

                try
                {
                    _ = webClient.UploadString(REVOKE_URL, string.Empty);
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
                    WaitForNextRequest();

                    result = webClient.DownloadString(USERS_URL);

                    SetNextRequestTime(webClient);
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
                                        WaitForNextRequest();

                                        _ = webClientChannel.DownloadString(CHANNELS_URL);

                                        SetNextRequestTime(webClient);

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

                    WaitForNextRequest();

                    string result = webClient.DownloadString(VIDEOS_URL);

                    SetNextRequestTime(webClient);

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
                            TimeSpan? startTime = GetStartTimeFromUrl(url);

                            if (startTime.HasValue)
                            {
                                video.StartTime = startTime;
                            }

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

        public TwitchVideoAuthInfo GetVodAuthInfo(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            using (WebClient webClient = CreateGqlWebClient())
            {
                var gqlPlaybackAccessToken = CreateGqlPlaybackAccessToken(id);
                string accessTokenStr = webClient.UploadString(GQL_URL, gqlPlaybackAccessToken);

                JObject accessTokenJson = JObject.Parse(accessTokenStr);

                JToken vpaToken = accessTokenJson.SelectToken("$.data.videoPlaybackAccessToken", false);

                if (vpaToken == null)
                {
                    throw new ApplicationException("The video playback access token is null!");
                }

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

                return new TwitchVideoAuthInfo(token, signature, privileged, subOnly);
            }
        }

        private string CreateGqlPlaybackAccessToken(string id)
        {
            //{
            //  "operationName": "PlaybackAccessToken_Template",
            //  "query": "query PlaybackAccessToken_Template($login: String!, $isLive: Boolean!, $vodID: ID!, $isVod: Boolean!, $playerType: String!) {  streamPlaybackAccessToken(channelName: $login, params: {platform: \"web\", playerBackend: \"mediaplayer\", playerType: $playerType}) @include(if: $isLive) {    value    signature    __typename  }  videoPlaybackAccessToken(id: $vodID, params: {platform: \"web\", playerBackend: \"mediaplayer\", playerType: $playerType}) @include(if: $isVod) {    value    signature    __typename  }}",
            //  "variables": {
            //    "isLive": false,
            //    "login": "",
            //    "isVod": true,
            //    "vodID": "1058435233",
            //    "playerType": "site"
            //  }
            //}

            return "{\"operationName\": \"PlaybackAccessToken_Template\", \"query\": \"query PlaybackAccessToken_Template($login: String!, $isLive: Boolean!, $vodID: ID!, $isVod: Boolean!, $playerType: String!) { streamPlaybackAccessToken(channelName: $login, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isLive) {    value    signature    __typename  }  videoPlaybackAccessToken(id: $vodID, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isVod) {    value    signature    __typename  }}\", \"variables\": { \"isLive\": false, \"login\": \"\", \"isVod\": true, \"vodID\": \"" + id + "\", \"playerType\": \"site\" }}";
        }

        public Dictionary<TwitchVideoQuality, string> GetPlaylistInfo(string vodId, TwitchVideoAuthInfo vodAuthInfo)
        {
            using (WebClient webClient = CreateWebClientWithEncoding())
            {
                webClient.Headers.Add("Accept", "*/*");

                webClient.QueryString.Add("allow_source", "true");
                webClient.QueryString.Add("allow_audio_only", "true");
                webClient.QueryString.Add("token", vodAuthInfo.Token);
                webClient.QueryString.Add("sig", vodAuthInfo.Signature);

                string playlist = webClient.DownloadString(string.Format(PLAYLISTS_URL, vodId));

                List<string> lines = playlist.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                Dictionary<TwitchVideoQuality, string> playlistInfo = new Dictionary<TwitchVideoQuality, string>();

                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];

                    if (!line.StartsWith("#"))
                    {
                        string mediaInfo = lines[i - 2];
                        string streamInfo = lines[i - 1];

                        Match groupMatch = _rxGroup.Match(mediaInfo);
                        string id = groupMatch.Groups["group"].Value;

                        Match nameMatch = _rxName.Match(mediaInfo);
                        string name = nameMatch.Groups["name"].Value;

                        string resolution = null;

                        Match resolutionMatch = _rxResolution.Match(streamInfo);

                        if (resolutionMatch.Success)
                        {
                            resolution = resolutionMatch.Groups["resolution"].Value;
                        }

                        TwitchVideoQuality quality = new TwitchVideoQuality(id, name, resolution);

                        playlistInfo.Add(quality, line);
                    }
                }

                return playlistInfo;
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

        private TimeSpan? GetStartTimeFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri validUrl))
            {
                return null;
            }

            string query = validUrl.Query;

            if (!string.IsNullOrWhiteSpace(query))
            {
                var queryParams = HttpUtility.ParseQueryString(validUrl.Query);

                if (queryParams.ContainsKey("t"))
                {
                    string durationStr = queryParams["t"];

                    if (!string.IsNullOrWhiteSpace(durationStr) && TimeSpanExtensions.TryParseTwitchFormat(durationStr, out TimeSpan startTime))
                    {
                        return startTime;
                    }
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
            TimeSpan length = TimeSpanExtensions.ParseTwitchFormat(videoJson.Value<string>("duration"));
            JArray mutedSegments = videoJson.Value<JArray>("muted_segments");

            string recorded = videoJson.Value<string>("published_at");

            if (string.IsNullOrWhiteSpace(recorded))
            {
                recorded = videoJson.Value<string>("created_at");
            }

            DateTime recordedDate = DateTime.Parse(recorded, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

            bool muted = mutedSegments != null && mutedSegments.Count > 0;
            bool live = false;

            if (string.IsNullOrWhiteSpace(thumbnail))
            {
                thumbnail = PROCESSING_THUMBNAIL;
                live = true;
            }
            else
            {
                thumbnail = thumbnail.Replace("%{width}", "320").Replace("%{height}", "180");
            }

            return new TwitchVideo(channel, title, id, views, length, recordedDate, new Uri(thumbnail), new Uri(url), viewable, muted, live);
        }

        #endregion Methods
    }
}