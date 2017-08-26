using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Events;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Notification;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Services.Services
{
    internal class TwitchService : BindableBase, ITwitchService, IDisposable
    {
        #region Constants

        private const string KRAKEN_URL = "https://api.twitch.tv/kraken";
        private const string VIDEO_URL = "https://api.twitch.tv/kraken/videos/{0}";
        private const string CHANNEL_SEARCH_URL = "https://api.twitch.tv/kraken/search/channels";
        private const string CHANNEL_VIDEOS_URL = "https://api.twitch.tv/kraken/channels/{0}/videos";
        private const string ACCESS_TOKEN_URL = "https://api.twitch.tv/api/vods/{0}/access_token";
        private const string ALL_PLAYLISTS_URL = "https://usher.twitch.tv/vod/{0}?nauthsig={1}&nauth={2}&allow_source=true&player=twitchweb&allow_spectre=true&allow_audio_only=true";

        private const string TEMP_PREFIX = "TL_";
        private const string PLAYLIST_NAME = "vod.m3u8";
        private const string FFMPEG_EXE_X86 = "ffmpeg_x86.exe";
        private const string FFMPEG_EXE_X64 = "ffmpeg_x64.exe";

        private const int TIMER_INTERVALL = 2;
        private const int DOWNLOAD_RETRIES = 3;
        private const int DOWNLOAD_RETRY_TIME = 20;

        private const int TWITCH_MAX_LOAD_LIMIT = 100;
        private const int TWITCH_IMG_WIDTH = 240;
        private const int TWITCH_IMG_HEIGHT = 135;

        private const string TWITCH_CLIENT_ID = "37v97169hnj8kaoq8fs3hzz8v6jezdj";
        private const string TWITCH_CLIENT_ID_HEADER = "Client-ID";
        private const string TWITCH_V5_ACCEPT = "application/vnd.twitchtv.v5+json";
        private const string TWITCH_V5_ACCEPT_HEADER = "Accept";
        private const string TWITCH_AUTHORIZATION_HEADER = "Authorization";

        #endregion Constants

        #region Fields

        private bool disposedValue = false;

        private IPreferencesService _preferencesService;
        private IRuntimeDataService _runtimeDataService;
        private IEventAggregator _eventAggregator;

        private Timer _downloadTimer;

        private ObservableCollection<TwitchVideo> _videos;
        private ObservableCollection<TwitchVideoDownload> _downloads;

        private ConcurrentDictionary<string, DownloadTask> _downloadTasks;
        private TwitchAuthInfo _twitchAuthInfo;

        private string _appDir;

        private object _changeDownloadLockObject;

        private volatile bool _paused;

        #endregion Fields

        #region Constructors

        public TwitchService(
            IPreferencesService preferencesService,
            IRuntimeDataService runtimeDataService,
            IEventAggregator eventAggregator)
        {
            _preferencesService = preferencesService;
            _runtimeDataService = runtimeDataService;
            _eventAggregator = eventAggregator;

            _videos = new ObservableCollection<TwitchVideo>();
            _videos.CollectionChanged += Videos_CollectionChanged;

            _downloads = new ObservableCollection<TwitchVideoDownload>();
            _downloads.CollectionChanged += Downloads_CollectionChanged;

            _downloadTasks = new ConcurrentDictionary<string, DownloadTask>();

            _appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            _changeDownloadLockObject = new object();

            _downloadTimer = new Timer(DownloadTimerCallback, null, 0, TIMER_INTERVALL);

            _eventAggregator.GetEvent<RemoveDownloadEvent>().Subscribe(Remove, ThreadOption.UIThread);
        }

        #endregion Constructors

        #region Properties

        public bool IsAuthorized
        {
            get
            {
                return _twitchAuthInfo != null;
            }
        }

        public ObservableCollection<TwitchVideo> Videos
        {
            get
            {
                return _videos;
            }
            private set
            {
                if (_videos != null)
                {
                    _videos.CollectionChanged -= Videos_CollectionChanged;
                }

                SetProperty(ref _videos, value, nameof(Videos));

                if (_videos != null)
                {
                    _videos.CollectionChanged += Videos_CollectionChanged;
                }

                FireVideosCountChanged();
            }
        }

        public ObservableCollection<TwitchVideoDownload> Downloads
        {
            get
            {
                return _downloads;
            }
            private set
            {
                if (_downloads != null)
                {
                    _downloads.CollectionChanged -= Downloads_CollectionChanged;
                }

                SetProperty(ref _downloads, value, nameof(Downloads));

                if (_downloads != null)
                {
                    _downloads.CollectionChanged += Downloads_CollectionChanged;
                }

                FireDownloadsCountChanged();
            }
        }

        #endregion Properties

        #region Methods

        private WebClient CreateTwitchWebClient()
        {
            WebClient wc = new WebClient();
            wc.Headers.Add(TWITCH_CLIENT_ID_HEADER, TWITCH_CLIENT_ID);
            wc.Headers.Add(TWITCH_V5_ACCEPT_HEADER, TWITCH_V5_ACCEPT);

            if (IsAuthorized)
            {
                wc.Headers.Add(TWITCH_AUTHORIZATION_HEADER, "OAuth " + _twitchAuthInfo.AccessToken);
            }

            wc.Encoding = Encoding.UTF8;
            return wc;
        }

        public VodAuthInfo RetrieveVodAuthInfo(string idTrimmed)
        {
            if (string.IsNullOrWhiteSpace(idTrimmed))
            {
                throw new ArgumentNullException(nameof(idTrimmed));
            }

            using (WebClient webClient = CreateTwitchWebClient())
            {
                string accessTokenStr = webClient.DownloadString(string.Format(ACCESS_TOKEN_URL, idTrimmed));

                JObject accessTokenJson = JObject.Parse(accessTokenStr);

                string token = Uri.EscapeDataString(accessTokenJson.Value<string>("token"));
                string signature = accessTokenJson.Value<string>("sig");

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

            Func<string> searchForChannel = () =>
            {
                using (WebClient webClient = CreateTwitchWebClient())
                {
                    webClient.QueryString.Add("query", channel);
                    webClient.QueryString.Add("limit", "1");

                    string result = webClient.DownloadString(CHANNEL_SEARCH_URL);

                    JObject searchResultJson = JObject.Parse(result);

                    JArray channelsJson = searchResultJson.Value<JArray>("channels");

                    if (channelsJson != null && channelsJson.HasValues)
                    {
                        foreach (JObject channelJson in channelsJson)
                        {
                            if (channelJson.Value<string>("display_name").Equals(channel, StringComparison.OrdinalIgnoreCase))
                            {
                                return channelJson.Value<string>("_id");
                            }
                        }
                    }

                    return null;
                }
            };

            // The Twitch search API has a bug where it sometimes returns an empty channel list
            // To work against that, retry searching for the channel name at least 5 times

            for (int i = 0; i < 5; i++)
            {
                string id = searchForChannel();

                if (id != null)
                {
                    return id;
                }
            }

            return null;
        }

        public bool Authorize(string accessToken)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                using (WebClient webClient = CreateTwitchWebClient())
                {
                    webClient.Headers.Add(TWITCH_AUTHORIZATION_HEADER, "OAuth " + accessToken);

                    string result = webClient.DownloadString(KRAKEN_URL);

                    JObject verifyRequestJson = JObject.Parse(result);

                    if (verifyRequestJson != null)
                    {
                        JObject tokenJson = verifyRequestJson.Value<JObject>("token");

                        if (tokenJson != null)
                        {
                            bool valid = tokenJson.Value<bool>("valid");

                            if (valid)
                            {
                                string username = tokenJson.Value<string>("user_name");
                                string clientId = tokenJson.Value<string>("client_id");

                                if (!string.IsNullOrWhiteSpace(username) &&
                                    !string.IsNullOrWhiteSpace(clientId) &&
                                    clientId.Equals(TWITCH_CLIENT_ID, StringComparison.OrdinalIgnoreCase))
                                {
                                    _twitchAuthInfo = new TwitchAuthInfo(accessToken, username);
                                    FireIsAuthorizedChanged();
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            RevokeAuthorization();
            return false;
        }

        public void RevokeAuthorization()
        {
            _twitchAuthInfo = null;
            FireIsAuthorizedChanged();
        }

        public void Search(SearchParameters searchParams)
        {
            if (searchParams == null)
            {
                throw new ArgumentNullException(nameof(searchParams));
            }

            switch (searchParams.SearchType)
            {
                case SearchType.Channel:
                    SearchChannel(searchParams.Channel, searchParams.VideoType, searchParams.LoadLimit, searchParams.LoadFrom.Value, searchParams.LoadTo.Value, searchParams.LoadLastVods);
                    break;

                case SearchType.Urls:
                    SearchUrls(searchParams.Urls);
                    break;

                case SearchType.Ids:
                    SearchIds(searchParams.Ids);
                    break;
            }
        }

        private void SearchChannel(string channel, VideoType videoType, LoadLimit loadLimit, DateTime loadFrom, DateTime loadTo, int loadLastVods)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentNullException(nameof(channel));
            }

            string channelId = GetChannelIdByName(channel);

            ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

            string broadcastTypeParam = null;

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

            string channelVideosUrl = string.Format(CHANNEL_VIDEOS_URL, channelId);

            DateTime fromDate = DateTime.Now;
            DateTime toDate = DateTime.Now;

            if (loadLimit == LoadLimit.Timespan)
            {
                fromDate = loadFrom;
                toDate = loadTo;
            }

            int offset = 0;
            int total = 0;
            int sum = 0;

            bool stop = false;

            do
            {
                using (WebClient webClient = CreateTwitchWebClient())
                {
                    webClient.QueryString.Add("broadcast_type", broadcastTypeParam);
                    webClient.QueryString.Add("limit", TWITCH_MAX_LOAD_LIMIT.ToString());
                    webClient.QueryString.Add("offset", offset.ToString());

                    string result = webClient.DownloadString(channelVideosUrl);

                    JObject videosResponseJson = JObject.Parse(result);

                    if (videosResponseJson != null)
                    {
                        if (total == 0)
                        {
                            total = videosResponseJson.Value<int>("_total");
                        }

                        foreach (JObject videoJson in videosResponseJson.Value<JArray>("videos"))
                        {
                            sum++;

                            if (videoJson.Value<string>("_id").StartsWith("v"))
                            {
                                TwitchVideo video = ParseVideo(videoJson);

                                if (loadLimit == LoadLimit.LastVods)
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

                offset += TWITCH_MAX_LOAD_LIMIT;
            } while (!stop && sum < total);

            Videos = videos;
        }

        private void SearchUrls(string urls)
        {
            if (string.IsNullOrWhiteSpace(urls))
            {
                throw new ArgumentNullException(nameof(urls));
            }

            ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

            string[] urlArr = urls.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            if (urlArr.Length > 0)
            {
                HashSet<int> addedIds = new HashSet<int>();

                foreach (string url in urlArr)
                {
                    int? id = GetVideoIdFromUrl(url);

                    if (id.HasValue && !addedIds.Contains(id.Value))
                    {
                        TwitchVideo video = GetTwitchVideoFromId(id.Value);

                        if (video != null)
                        {
                            videos.Add(video);
                            addedIds.Add(id.Value);
                        }
                    }
                }
            }

            Videos = videos;
        }

        private void SearchIds(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                throw new ArgumentNullException(nameof(ids));
            }

            ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

            string[] idsArr = ids.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            if (idsArr.Length > 0)
            {
                HashSet<int> addedIds = new HashSet<int>();

                foreach (string id in idsArr)
                {
                    if (int.TryParse(id, out int idInt) && !addedIds.Contains(idInt))
                    {
                        TwitchVideo video = GetTwitchVideoFromId(idInt);

                        if (video != null)
                        {
                            videos.Add(video);
                            addedIds.Add(idInt);
                        }
                    }
                }
            }

            Videos = videos;
        }

        private int? GetVideoIdFromUrl(string url)
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
                if (segments[i].Equals("videos/", StringComparison.OrdinalIgnoreCase))
                {
                    if (segments.Length > (i + 1))
                    {
                        string idStr = segments[i + 1];

                        if (!string.IsNullOrWhiteSpace(idStr))
                        {
                            idStr = idStr.Trim(new char[] { '/' });

                            if (int.TryParse(idStr, out int idInt) && idInt > 0)
                            {
                                return idInt;
                            }
                        }
                    }

                    break;
                }
            }

            return null;
        }

        private TwitchVideo GetTwitchVideoFromId(int id)
        {
            using (WebClient webClient = CreateTwitchWebClient())
            {
                try
                {
                    string result = webClient.DownloadString(string.Format(VIDEO_URL, id));

                    JObject videoJson = JObject.Parse(result);

                    if (videoJson != null)
                    {
                        return ParseVideo(videoJson);
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

        public void Enqueue(DownloadParameters downloadParams)
        {
            if (_paused)
            {
                return;
            }

            lock (_changeDownloadLockObject)
            {
                _downloads.Add(new TwitchVideoDownload(downloadParams));
            }
        }

        private void DownloadTimerCallback(object state)
        {
            if (_paused)
            {
                return;
            }

            StartQueuedDownloadIfExists();
        }

        private void StartQueuedDownloadIfExists()
        {
            if (_paused)
            {
                return;
            }

            if (Monitor.TryEnter(_changeDownloadLockObject))
            {
                try
                {
                    if (!_downloads.Where(d => d.DownloadState == DownloadState.Downloading).Any())
                    {
                        TwitchVideoDownload download = _downloads.Where(d => d.DownloadState == DownloadState.Queued).FirstOrDefault();

                        if (download == null)
                        {
                            return;
                        }

                        DownloadParameters downloadParams = download.DownloadParams;

                        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                        CancellationToken cancellationToken = cancellationTokenSource.Token;

                        string downloadId = download.Id;
                        string urlIdTrimmed = downloadParams.Video.IdTrimmed;
                        string tempDir = Path.Combine(_preferencesService.CurrentPreferences.DownloadTempFolder, TEMP_PREFIX + downloadId);
                        string playlistFile = Path.Combine(tempDir, PLAYLIST_NAME);
                        string ffmpegFile = Path.Combine(_appDir, Environment.Is64BitOperatingSystem ? FFMPEG_EXE_X64 : FFMPEG_EXE_X86);
                        string outputFile = downloadParams.FullPath;

                        bool cropStart = downloadParams.CropStart;
                        bool cropEnd = downloadParams.CropEnd;

                        TimeSpan cropStartTime = downloadParams.CropStartTime;
                        TimeSpan cropEndTime = downloadParams.CropEndTime;

                        TwitchVideoQuality quality = downloadParams.Quality;

                        VodAuthInfo vodAuthInfo = downloadParams.VodAuthInfo;

                        Action<DownloadState> setDownloadState = download.SetDownloadState;
                        Action<string> log = download.AppendLog;
                        Action<string> setStatus = download.SetStatus;
                        Action<int> setProgress = download.SetProgress;
                        Action<bool> setIsProcessing = download.SetIsProcessing;

                        Task downloadVideoTask = new Task(() =>
                        {
                            setStatus("Initializing");

                            log("Download task has been started!");

                            WriteDownloadInfo(log, downloadParams, ffmpegFile, tempDir);

                            CheckTempDirectory(log, tempDir);

                            cancellationToken.ThrowIfCancellationRequested();

                            string playlistUrl = RetrievePlaylistUrlForQuality(log, quality, urlIdTrimmed, vodAuthInfo);

                            cancellationToken.ThrowIfCancellationRequested();

                            VodPlaylist vodPlaylist = RetrieveVodPlaylist(log, tempDir, playlistUrl);

                            cancellationToken.ThrowIfCancellationRequested();

                            CropInfo cropInfo = CropVodPlaylist(vodPlaylist, cropStart, cropEnd, cropStartTime, cropEndTime);

                            cancellationToken.ThrowIfCancellationRequested();

                            DownloadParts(log, setStatus, setProgress, vodPlaylist, cancellationToken);

                            cancellationToken.ThrowIfCancellationRequested();

                            WriteNewPlaylist(log, vodPlaylist, playlistFile);

                            cancellationToken.ThrowIfCancellationRequested();

                            ProcessVideo(log, setStatus, setProgress, setIsProcessing, ffmpegFile, playlistFile, outputFile, cropInfo);
                        }, cancellationToken);

                        Task continueTask = downloadVideoTask.ContinueWith(task =>
                        {
                            log(Environment.NewLine + Environment.NewLine + "Starting temporary download folder cleanup!");
                            CleanUp(tempDir, log);

                            setProgress(100);
                            setIsProcessing(false);

                            bool success = false;

                            if (task.IsFaulted)
                            {
                                setDownloadState(DownloadState.Error);
                                log(Environment.NewLine + Environment.NewLine + "Download task ended with an error!");

                                if (task.Exception != null)
                                {
                                    log(Environment.NewLine + Environment.NewLine + task.Exception.ToString());
                                }
                            }
                            else if (task.IsCanceled)
                            {
                                setDownloadState(DownloadState.Canceled);
                                log(Environment.NewLine + Environment.NewLine + "Download task was canceled!");
                            }
                            else
                            {
                                success = true;
                                setDownloadState(DownloadState.Done);
                                log(Environment.NewLine + Environment.NewLine + "Download task ended successfully!");
                            }

                            if (!_downloadTasks.TryRemove(downloadId, out DownloadTask downloadTask))
                            {
                                throw new ApplicationException("Could not remove download task with ID '" + downloadId + "' from download task collection!");
                            }

                            if (success && _preferencesService.CurrentPreferences.DownloadRemoveCompleted)
                            {
                                _eventAggregator.GetEvent<RemoveDownloadEvent>().Publish(downloadId);
                            }
                        });

                        if (_downloadTasks.TryAdd(downloadId, new DownloadTask(downloadVideoTask, continueTask, cancellationTokenSource)))
                        {
                            downloadVideoTask.Start();
                            setDownloadState(DownloadState.Downloading);
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_changeDownloadLockObject);
                }
            }
        }

        private void WriteDownloadInfo(Action<string> log, DownloadParameters downloadParams, string ffmpegFile, string tempDir)
        {
            log(Environment.NewLine + Environment.NewLine + "TWITCH LEECHER INFO");
            log(Environment.NewLine + "--------------------------------------------------------------------------------------------");
            log(Environment.NewLine + "Version: " + AssemblyUtil.Get.GetAssemblyVersion().Trim());

            log(Environment.NewLine + Environment.NewLine + "VOD INFO");
            log(Environment.NewLine + "--------------------------------------------------------------------------------------------");
            log(Environment.NewLine + "VOD ID: " + downloadParams.Video.IdTrimmed);
            log(Environment.NewLine + "Selected Quality: " + downloadParams.Quality.DisplayString);
            log(Environment.NewLine + "Download Url: " + downloadParams.Video.Url);
            log(Environment.NewLine + "Crop Start: " + (downloadParams.CropStart ? "Yes (" + downloadParams.CropStartTime.ToDaylessString() + ")" : "No"));
            log(Environment.NewLine + "Crop End: " + (downloadParams.CropEnd ? "Yes (" + downloadParams.CropEndTime.ToDaylessString() + ")" : "No"));

            log(Environment.NewLine + Environment.NewLine + "OUTPUT INFO");
            log(Environment.NewLine + "--------------------------------------------------------------------------------------------");
            log(Environment.NewLine + "Output File: " + downloadParams.FullPath);
            log(Environment.NewLine + "FFMPEG Path: " + ffmpegFile);
            log(Environment.NewLine + "Temporary Download Folder: " + tempDir);

            VodAuthInfo vodAuthInfo = downloadParams.VodAuthInfo;

            log(Environment.NewLine + Environment.NewLine + "ACCESS INFO");
            log(Environment.NewLine + "--------------------------------------------------------------------------------------------");
            log(Environment.NewLine + "Token: " + vodAuthInfo.Token);
            log(Environment.NewLine + "Signature: " + vodAuthInfo.Signature);
            log(Environment.NewLine + "Sub-Only: " + (vodAuthInfo.SubOnly ? "Yes" : "No"));
            log(Environment.NewLine + "Privileged: " + (vodAuthInfo.Privileged ? "Yes" : "No"));
        }

        private void CheckTempDirectory(Action<string> log, string tempDir)
        {
            if (!Directory.Exists(tempDir))
            {
                log(Environment.NewLine + Environment.NewLine + "Creating directory '" + tempDir + "'...");
                FileSystem.CreateDirectory(tempDir);
                log(" done!");
            }

            if (Directory.EnumerateFileSystemEntries(tempDir).Any())
            {
                throw new ApplicationException("Temporary download directory '" + tempDir + "' is not empty!");
            }
        }

        private string RetrievePlaylistUrlForQuality(Action<string> log, TwitchVideoQuality quality, string urlIdTrimmed, VodAuthInfo vodAuthInfo)
        {
            using (WebClient webClient = CreateTwitchWebClient())
            {
                log(Environment.NewLine + Environment.NewLine + "Retrieving m3u8 playlist urls for all VOD qualities...");
                string allPlaylistsStr = webClient.DownloadString(string.Format(ALL_PLAYLISTS_URL, urlIdTrimmed, vodAuthInfo.Signature, vodAuthInfo.Token));
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

                log(Environment.NewLine + "Number of video chunks: " + vodPlaylist.OfType<IVodPlaylistPartExt>().Count());

                return vodPlaylist;
            }
        }

        private CropInfo CropVodPlaylist(VodPlaylist vodPlaylist, bool cropStart, bool cropEnd, TimeSpan cropStartTime, TimeSpan cropEndTime)
        {
            double start = cropStartTime.TotalMilliseconds;
            double end = cropEndTime.TotalMilliseconds;
            double length = cropEndTime.TotalMilliseconds;

            if (cropStart)
            {
                length -= start;
            }

            start = Math.Round(start / 1000, 3);
            end = Math.Round(end / 1000, 3);
            length = Math.Round(length / 1000, 3);

            List<IVodPlaylistPartExt> parts = vodPlaylist.OfType<IVodPlaylistPartExt>().ToList();

            int firstPartIndex = parts.First().Index;
            int lastPartIndex = parts.Last().Index;

            List<IVodPlaylistPartExt> deleteStart = new List<IVodPlaylistPartExt>();
            List<IVodPlaylistPartExt> deleteEnd = new List<IVodPlaylistPartExt>();

            if (cropStart)
            {
                double lengthSum = 0;

                foreach (IVodPlaylistPartExt part in vodPlaylist.OfType<IVodPlaylistPartExt>())
                {
                    double partLength = part.Length;

                    if (lengthSum + partLength < start)
                    {
                        lengthSum += partLength;
                        deleteStart.Add(part);
                    }
                    else
                    {
                        start = Math.Round(start - lengthSum, 3);
                        break;
                    }
                }
            }

            if (cropEnd)
            {
                double lengthSum = 0;

                foreach (IVodPlaylistPartExt part in vodPlaylist.OfType<IVodPlaylistPartExt>())
                {
                    if (lengthSum >= end)
                    {
                        deleteEnd.Add(part);
                    }

                    lengthSum += part.Length;
                }
            }

            deleteStart.ForEach(part =>
            {
                vodPlaylist.Remove(part);
            });

            deleteEnd.ForEach(part =>
            {
                vodPlaylist.Remove(part);
            });

            List<IVodPlaylistPartExt> partsCropped = vodPlaylist.OfType<IVodPlaylistPartExt>().ToList();

            int firstPartCroppedIndex = partsCropped.First().Index;
            int lastPartCroppedIndex = partsCropped.Last().Index;

            List<IVodPlaylistPart> deleteInfo = new List<IVodPlaylistPart>();

            foreach (IVodPlaylistPart part in vodPlaylist.OfType<IVodPlaylistPart>())
            {
                int index = part.Index;

                if ((index > firstPartIndex && index < firstPartCroppedIndex) ||
                    (index > lastPartCroppedIndex && index < lastPartIndex))
                {
                    deleteInfo.Add(part);
                }
            }

            deleteInfo.ForEach(part =>
            {
                vodPlaylist.Remove(part);
            });

            return new CropInfo(cropStart, cropEnd, cropStart ? start : 0, length);
        }

        private void DownloadParts(Action<string> log, Action<string> setStatus, Action<int> setProgress,
            VodPlaylist vodPlaylist, CancellationToken cancellationToken)
        {
            List<IVodPlaylistPartExt> parts = vodPlaylist.OfType<IVodPlaylistPartExt>().ToList();

            int partsCount = parts.Count;
            int maxConnectionCount = ServicePointManager.DefaultConnectionLimit;

            log(Environment.NewLine + Environment.NewLine + "Starting parallel video chunk download");
            log(Environment.NewLine + "Number of video chunks to download: " + partsCount);
            log(Environment.NewLine + "Maximum connection count: " + maxConnectionCount);

            setStatus("Downloading");

            log(Environment.NewLine + Environment.NewLine + "Parallel video chunk download is running...");

            long completedPartDownloads = 0;

            Parallel.ForEach(parts, new ParallelOptions() { MaxDegreeOfParallelism = maxConnectionCount - 1 }, (part, loopState) =>
            {
                int retryCounter = 0;

                bool success = false;

                do
                {
                    try
                    {
                        using (WebClient downloadClient = new WebClient())
                        {
                            byte[] bytes = downloadClient.DownloadData(part.DownloadUrl);

                            Interlocked.Increment(ref completedPartDownloads);

                            FileSystem.DeleteFile(part.LocalFile);

                            File.WriteAllBytes(part.LocalFile, bytes);

                            long completed = Interlocked.Read(ref completedPartDownloads);

                            setProgress((int)(completedPartDownloads * 100 / partsCount));

                            success = true;
                        }
                    }
                    catch (WebException ex)
                    {
                        if (retryCounter < DOWNLOAD_RETRIES)
                        {
                            retryCounter++;
                            log(Environment.NewLine + Environment.NewLine + "Downloading file '" + part.DownloadUrl + "' failed! Trying again in " + DOWNLOAD_RETRY_TIME + "s");
                            log(Environment.NewLine + ex.ToString());
                            Thread.Sleep(DOWNLOAD_RETRY_TIME * 1000);
                        }
                        else
                        {
                            throw new ApplicationException("Could not download file '" + part.DownloadUrl + "' after " + DOWNLOAD_RETRIES + " retries!");
                        }
                    }
                }
                while (!success);

                if (cancellationToken.IsCancellationRequested)
                {
                    loopState.Stop();
                }
            });

            setProgress(100);

            log(Environment.NewLine + Environment.NewLine + "Download of all video chunks complete!");
        }

        private void WriteNewPlaylist(Action<string> log, VodPlaylist vodPlaylist, string playlistFile)
        {
            log(Environment.NewLine + Environment.NewLine + "Creating local m3u8 playlist for FFMPEG...");

            StringBuilder sb = new StringBuilder();

            vodPlaylist.ForEach(part =>
            {
                sb.AppendLine(part.GetOutput());
            });

            log(" done!");

            log(Environment.NewLine + "Writing playlist to '" + playlistFile + "'...");
            FileSystem.DeleteFile(playlistFile);
            File.WriteAllText(playlistFile, sb.ToString());
            log(" done!");
        }

        private void ProcessVideo(Action<string> log, Action<string> setStatus, Action<int> setProgress,
            Action<bool> setIsProcessing, string ffmpegFile, string playlistFile, string outputFile, CropInfo cropInfo)
        {
            setStatus("Processing");
            setIsProcessing(true);

            log(Environment.NewLine + Environment.NewLine + "Executing '" + ffmpegFile + "' on local playlist...");

            ProcessStartInfo psi = new ProcessStartInfo(ffmpegFile)
            {
                Arguments = "-y" + (cropInfo.CropStart ? " -ss " + cropInfo.Start.ToString(CultureInfo.InvariantCulture) : null) + " -i \"" + playlistFile + "\" -analyzeduration " + int.MaxValue + " -probesize " + int.MaxValue + " -c:v copy -c:a copy -bsf:a aac_adtstoasc" + (cropInfo.CropEnd ? " -t " + cropInfo.Length.ToString(CultureInfo.InvariantCulture) : null) + " \"" + outputFile + "\"",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            log(Environment.NewLine + "Command line arguments: " + psi.Arguments + Environment.NewLine);

            using (Process p = new Process())
            {
                TimeSpan duration = TimeSpan.FromSeconds(cropInfo.Length);

                DataReceivedEventHandler outputDataReceived = new DataReceivedEventHandler((s, e) =>
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            string dataTrimmed = e.Data.Trim();

                            if (dataTrimmed.StartsWith("frame", StringComparison.OrdinalIgnoreCase) && duration != TimeSpan.Zero)
                            {
                                string timeStr = dataTrimmed.Substring(dataTrimmed.IndexOf("time") + 4).Trim();
                                timeStr = timeStr.Substring(timeStr.IndexOf("=") + 1).Trim();
                                timeStr = timeStr.Substring(0, timeStr.IndexOf(" ")).Trim();

                                if (TimeSpan.TryParse(timeStr, out TimeSpan current))
                                {
                                    setIsProcessing(false);
                                    setProgress((int)(current.TotalMilliseconds * 100 / duration.TotalMilliseconds));
                                }
                                else
                                {
                                    setIsProcessing(true);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log(Environment.NewLine + "An error occured while reading '" + ffmpegFile + "' output stream!" + Environment.NewLine + Environment.NewLine + ex.ToString());
                    }
                });

                p.OutputDataReceived += outputDataReceived;
                p.ErrorDataReceived += outputDataReceived;
                p.StartInfo = psi;
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();

                if (p.ExitCode == 0)
                {
                    log(Environment.NewLine + "Processing complete!");
                }
                else
                {
                    throw new ApplicationException("An error occured while processing the video!");
                }
            }
        }

        private void CleanUp(string directory, Action<string> log)
        {
            try
            {
                log(Environment.NewLine + "Deleting directory '" + directory + "'...");
                FileSystem.DeleteDirectory(directory);
                log(" done!");
            }
            catch
            {
            }
        }

        public void Cancel(string id)
        {
            lock (_changeDownloadLockObject)
            {
                if (_downloadTasks.TryGetValue(id, out DownloadTask downloadTask))
                {
                    downloadTask.CancellationTokenSource.Cancel();
                }
            }
        }

        public void Retry(string id)
        {
            if (_paused)
            {
                return;
            }

            lock (_changeDownloadLockObject)
            {
                if (!_downloadTasks.TryGetValue(id, out DownloadTask downloadTask))
                {
                    TwitchVideoDownload download = _downloads.Where(d => d.Id == id).FirstOrDefault();

                    if (download != null && (download.DownloadState == DownloadState.Canceled || download.DownloadState == DownloadState.Error))
                    {
                        download.ResetLog();
                        download.SetProgress(0);
                        download.SetDownloadState(DownloadState.Queued);
                        download.SetStatus("Initializing");
                    }
                }
            }
        }

        public void Remove(string id)
        {
            lock (_changeDownloadLockObject)
            {
                if (!_downloadTasks.TryGetValue(id, out DownloadTask downloadTask))
                {
                    TwitchVideoDownload download = _downloads.Where(d => d.Id == id).FirstOrDefault();

                    if (download != null)
                    {
                        _downloads.Remove(download);
                    }
                }
            }
        }

        public TwitchVideo ParseVideo(JObject videoJson)
        {
            string channel = videoJson.Value<JObject>("channel").Value<string>("display_name");
            string title = videoJson.Value<string>("title");
            string id = videoJson.Value<string>("_id");
            string game = videoJson.Value<string>("game");
            int views = videoJson.Value<int>("views");
            TimeSpan length = new TimeSpan(0, 0, videoJson.Value<int>("length"));
            List<TwitchVideoQuality> qualities = ParseQualities(videoJson.Value<JObject>("resolutions"), videoJson.Value<JObject>("fps"));
            DateTime recordedDate = DateTime.ParseExact(videoJson.Value<string>("published_at"), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            Uri url = new Uri(videoJson.Value<string>("url"));

            string imgUrlTemplate = videoJson.Value<JObject>("preview").Value<string>("template")
                .Replace("{width}", TWITCH_IMG_WIDTH.ToString())
                .Replace("{height}", TWITCH_IMG_HEIGHT.ToString());

            Uri thumbnail = new Uri(imgUrlTemplate);

            return new TwitchVideo(channel, title, id, game, views, length, qualities, recordedDate, thumbnail, url);
        }

        public List<TwitchVideoQuality> ParseQualities(JObject resolutionsJson, JObject fpsJson)
        {
            List<TwitchVideoQuality> qualities = new List<TwitchVideoQuality>();

            Dictionary<string, string> fpsList = new Dictionary<string, string>();

            if (fpsJson != null)
            {
                foreach (JProperty fps in fpsJson.Values<JProperty>())
                {
                    fpsList.Add(fps.Name, ((int)Math.Round(fps.Value.Value<double>(), 0)).ToString());
                }
            }

            if (resolutionsJson != null)
            {
                foreach (JProperty resolution in resolutionsJson.Values<JProperty>())
                {
                    string value = resolution.Value.Value<string>();
                    string qualityId = resolution.Name;
                    string fps = fpsList.ContainsKey(qualityId) ? fpsList[qualityId] : null;

                    qualities.Add(new TwitchVideoQuality(qualityId, value, fps));
                }
            }

            if (fpsList.ContainsKey(TwitchVideoQuality.QUALITY_AUDIO))
            {
                qualities.Add(new TwitchVideoQuality(TwitchVideoQuality.QUALITY_AUDIO));
            }

            if (!qualities.Any())
            {
                qualities.Add(new TwitchVideoQuality(TwitchVideoQuality.QUALITY_SOURCE));
            }

            qualities.Sort();

            return qualities;
        }

        public void Pause()
        {
            _paused = true;
            _downloadTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Resume()
        {
            _paused = false;
            _downloadTimer.Change(0, TIMER_INTERVALL);
        }

        public bool CanShutdown()
        {
            Monitor.Enter(_changeDownloadLockObject);

            try
            {
                return !_downloads.Where(d => d.DownloadState == DownloadState.Downloading || d.DownloadState == DownloadState.Queued).Any();
            }
            finally
            {
                Monitor.Exit(_changeDownloadLockObject);
            }
        }

        public void Shutdown()
        {
            Pause();

            foreach (DownloadTask downloadTask in _downloadTasks.Values)
            {
                downloadTask.CancellationTokenSource.Cancel();
            }

            List<Task> tasks = _downloadTasks.Values.Select(v => v.Task).ToList();
            tasks.AddRange(_downloadTasks.Values.Select(v => v.ContinueTask).ToList());

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception)
            {
                // Don't care about aborted tasks
            }

            List<string> toRemove = _downloads.Select(d => d.Id).ToList();

            foreach (string id in toRemove)
            {
                Remove(id);
            }
        }

        public bool IsFileNameUsed(string fullPath)
        {
            IEnumerable<TwitchVideoDownload> downloads = _downloads.Where(d => d.DownloadState == DownloadState.Downloading || d.DownloadState == DownloadState.Queued);

            foreach (TwitchVideoDownload download in downloads)
            {
                if (download.DownloadParams.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void FireIsAuthorizedChanged()
        {
            _runtimeDataService.RuntimeData.AccessToken = _twitchAuthInfo?.AccessToken;
            _runtimeDataService.Save();

            FirePropertyChanged(nameof(IsAuthorized));
            _eventAggregator.GetEvent<IsAuthorizedChangedEvent>().Publish(IsAuthorized);
        }

        private void FireVideosCountChanged()
        {
            _eventAggregator.GetEvent<VideosCountChangedEvent>().Publish(_videos != null ? _videos.Count : 0);
        }

        private void FireDownloadsCountChanged()
        {
            _eventAggregator.GetEvent<DownloadsCountChangedEvent>().Publish(_downloads != null ? _downloads.Count : 0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _downloadTimer.Dispose();
                }

                _videos = null;
                _downloads = null;
                _downloadTasks = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion Methods

        #region EventHandlers

        private void Videos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireVideosCountChanged();
        }

        private void Downloads_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireDownloadsCountChanged();
        }

        #endregion EventHandlers
    }
}