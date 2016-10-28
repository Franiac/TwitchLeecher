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
using TwitchLeecher.Services.Models;
using TwitchLeecher.Shared.Events;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Notification;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Services.Services
{
    internal class TwitchService : BindableBase, ITwitchService
    {
        #region Constants

        private const string krakenUrl = "https://api.twitch.tv/kraken";
        private const string videoUrl = "https://api.twitch.tv/kraken/videos/{0}";
        private const string channelsUrl = "https://api.twitch.tv/kraken/channels/{0}";
        private const string channelVideosUrl = "https://api.twitch.tv/kraken/channels/{0}/videos";
        private const string accessTokenUrl = "https://api.twitch.tv/api/vods/{0}/access_token";

        private const string allPlaylistsUrl = "https://usher.twitch.tv/vod/{0}?nauthsig={1}&nauth={2}&allow_source=true&player=twitchweb&allow_spectre=true&allow_audio_only=true";

        private const string TEMP_PREFIX = "TL_";
        private const string PLAYLIST_NAME = "vod.m3u8";
        private const string FFMPEG_EXE_X86 = "ffmpeg_x86.exe";
        private const string FFMPEG_EXE_X64 = "ffmpeg_x64.exe";

        private const int TIMER_INTERVALL = 2;
        private const int DOWNLOAD_RETRIES = 3;
        private const int DOWNLOAD_RETRY_TIME = 20;

        private const int TWITCH_MAX_LOAD_LIMIT = 100;
        private const string TWITCH_CLIENT_ID = "37v97169hnj8kaoq8fs3hzz8v6jezdj";
        private const string TWITCH_CLIENT_ID_HEADER = "Client-ID";
        private const string TWITCH_AUTHORIZATION_HEADER = "Authorization";

        #endregion Constants

        #region Fields

        private IPreferencesService preferencesService;
        private IRuntimeDataService runtimeDataService;
        private IEventAggregator eventAggregator;

        private Timer downloadTimer;

        private ObservableCollection<TwitchVideo> videos;
        private ObservableCollection<TwitchVideoDownload> downloads;

        private ConcurrentDictionary<string, DownloadTask> downloadTasks;
        private Dictionary<string, int> qualityOrderMap;
        private Dictionary<string, int> resolutionOrderMap;
        private TwitchAuthInfo twitchAuthInfo;

        private string appDir;

        private object changeDownloadLockObject;

        private volatile bool paused;

        #endregion Fields

        #region Constructors

        public TwitchService(
            IPreferencesService preferencesService,
            IRuntimeDataService runtimeDataService,
            IEventAggregator eventAggregator)
        {
            this.preferencesService = preferencesService;
            this.runtimeDataService = runtimeDataService;
            this.eventAggregator = eventAggregator;

            this.Videos = new ObservableCollection<TwitchVideo>();
            this.Downloads = new ObservableCollection<TwitchVideoDownload>();

            this.downloadTasks = new ConcurrentDictionary<string, DownloadTask>();

            this.appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            this.changeDownloadLockObject = new object();

            this.qualityOrderMap = new Dictionary<string, int>();
            this.qualityOrderMap.Add(TwitchVideoQuality.QUALITY_SOURCE, 0);
            this.qualityOrderMap.Add(TwitchVideoQuality.QUALITY_HIGH, 1);
            this.qualityOrderMap.Add(TwitchVideoQuality.QUALITY_MEDIUM, 2);
            this.qualityOrderMap.Add(TwitchVideoQuality.QUALITY_LOW, 3);
            this.qualityOrderMap.Add(TwitchVideoQuality.QUALITY_MOBILE, 4);
            this.qualityOrderMap.Add(TwitchVideoQuality.QUALITY_AUDIO, 5);

            this.resolutionOrderMap = new Dictionary<string, int>();
            this.resolutionOrderMap.Add(TwitchVideoQuality.QUALITY_1080P, 0);
            this.resolutionOrderMap.Add(TwitchVideoQuality.QUALITY_720P, 1);
            this.resolutionOrderMap.Add(TwitchVideoQuality.QUALITY_480P, 2);
            this.resolutionOrderMap.Add(TwitchVideoQuality.QUALITY_360P, 3);
            this.resolutionOrderMap.Add(TwitchVideoQuality.QUALITY_240P, 4);
            this.resolutionOrderMap.Add(TwitchVideoQuality.QUALITY_144P, 5);

            this.downloadTimer = new Timer(this.DownloadTimerCallback, null, 0, TIMER_INTERVALL);

            this.eventAggregator.GetEvent<RemoveDownloadEvent>().Subscribe(this.Remove, ThreadOption.UIThread);
        }

        #endregion Constructors

        #region Properties

        public bool IsAuthorized
        {
            get
            {
                return this.twitchAuthInfo != null;
            }
        }

        public ObservableCollection<TwitchVideo> Videos
        {
            get
            {
                return this.videos;
            }
            private set
            {
                if (this.videos != null)
                {
                    this.videos.CollectionChanged -= Videos_CollectionChanged;
                }

                this.SetProperty(ref this.videos, value, nameof(this.Videos));

                if (this.videos != null)
                {
                    this.videos.CollectionChanged += Videos_CollectionChanged;
                }

                this.FireVideosCountChanged();
            }
        }

        public ObservableCollection<TwitchVideoDownload> Downloads
        {
            get
            {
                return this.downloads;
            }
            private set
            {
                if (this.downloads != null)
                {
                    this.downloads.CollectionChanged -= Downloads_CollectionChanged;
                }

                this.SetProperty(ref this.downloads, value, nameof(this.Downloads));

                if (this.downloads != null)
                {
                    this.downloads.CollectionChanged += Downloads_CollectionChanged;
                }

                this.FireDownloadsCountChanged();
            }
        }

        #endregion Properties

        #region Methods

        private WebClient CreateTwitchWebClient()
        {
            WebClient wc = new WebClient();
            wc.Headers.Add(TWITCH_CLIENT_ID_HEADER, TWITCH_CLIENT_ID);

            if (this.IsAuthorized)
            {
                wc.Headers.Add(TWITCH_AUTHORIZATION_HEADER, "OAuth " + this.twitchAuthInfo.AccessToken);
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

            using (WebClient webClient = this.CreateTwitchWebClient())
            {
                string accessTokenStr = webClient.DownloadString(string.Format(accessTokenUrl, idTrimmed));

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

        public bool UserExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            using (WebClient webClient = this.CreateTwitchWebClient())
            {
                try
                {
                    string result = webClient.DownloadString(string.Format(channelsUrl, username));
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool Authorize(string accessToken)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                using (WebClient webClient = this.CreateTwitchWebClient())
                {
                    webClient.Headers.Add(TWITCH_AUTHORIZATION_HEADER, "OAuth " + accessToken);

                    string result = webClient.DownloadString(krakenUrl);

                    JObject verifyRequestJson = JObject.Parse(result);

                    if (verifyRequestJson != null)
                    {
                        bool identified = verifyRequestJson.Value<bool>("identified");

                        if (identified)
                        {
                            JObject tokenJson = verifyRequestJson.Value<JObject>("token");

                            if (tokenJson != null)
                            {
                                bool valid = tokenJson.Value<bool>("valid");

                                if (valid)
                                {
                                    string username = tokenJson.Value<string>("user_name");

                                    if (!string.IsNullOrWhiteSpace(username))
                                    {
                                        this.twitchAuthInfo = new TwitchAuthInfo(accessToken, username);
                                        this.FireIsAuthorizedChanged();
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            this.RevokeAuthorization();
            return false;
        }

        public void RevokeAuthorization()
        {
            this.twitchAuthInfo = null;
            this.FireIsAuthorizedChanged();
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
                    this.SearchChannel(searchParams.Username, searchParams.VideoType, searchParams.LoadLimit);
                    break;

                case SearchType.Urls:
                    this.SearchUrls(searchParams.Urls);
                    break;

                case SearchType.Ids:
                    this.SearchIds(searchParams.Ids);
                    break;
            }
        }

        private void SearchChannel(string username, VideoType videoType, int loadLimit)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            using (WebClient webClient = this.CreateTwitchWebClient())
            {
                ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

                if (videoType == VideoType.Broadcast)
                {
                    webClient.QueryString.Add("broadcasts", "true");
                }

                webClient.QueryString.Add("limit", (loadLimit > TWITCH_MAX_LOAD_LIMIT ? TWITCH_MAX_LOAD_LIMIT.ToString() : loadLimit.ToString()));

                string result = webClient.DownloadString(string.Format(channelVideosUrl, username));

                JObject videoRequestJson = JObject.Parse(result);

                if (videoRequestJson != null)
                {
                    List<JArray> videoArrays = new List<JArray>();

                    JArray firstArray = videoRequestJson.Value<JArray>("videos");

                    videoArrays.Add(firstArray);

                    int sum = firstArray.Count;
                    int total = videoRequestJson.Value<int>("_total");

                    if (loadLimit > TWITCH_MAX_LOAD_LIMIT && sum < total)
                    {
                        JObject linksJson = videoRequestJson.Value<JObject>("_links");

                        string nextUrl = linksJson.Value<string>("next");

                        webClient.QueryString.Clear();

                        this.LoadVideosRecursive(webClient, nextUrl, loadLimit, sum, total, videoArrays);
                    }

                    foreach (JArray videoArray in videoArrays)
                    {
                        foreach (JObject videoJson in videoArray)
                        {
                            if (videoJson.Value<string>("_id").StartsWith("v"))
                            {
                                videos.Add(this.ParseVideo(videoJson));
                            }
                        }
                    }
                }

                this.Videos = videos;
            }
        }

        private void SearchUrls(string urls)
        {
            if (string.IsNullOrWhiteSpace(urls))
            {
                throw new ArgumentNullException(nameof(urls));
            }

            using (WebClient webClient = this.CreateTwitchWebClient())
            {
                ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

                string[] urlArr = urls.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                if (urlArr.Length > 0)
                {
                    HashSet<string> addedIds = new HashSet<string>();

                    foreach (string url in urlArr)
                    {
                        string id = this.GetVideoIdFromUrl(url);

                        if (!string.IsNullOrWhiteSpace(id) && !addedIds.Contains(id))
                        {
                            TwitchVideo video = this.GetTwitchVideoFromId(webClient, id);

                            if (video != null)
                            {
                                videos.Add(video);
                                addedIds.Add(id);
                            }
                        }
                    }
                }

                this.Videos = videos;
            }
        }

        private void SearchIds(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                throw new ArgumentNullException(nameof(ids));
            }

            using (WebClient webClient = this.CreateTwitchWebClient())
            {
                ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

                string[] idsArr = ids.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                if (idsArr.Length > 0)
                {
                    HashSet<int> addedIds = new HashSet<int>();

                    foreach (string id in idsArr)
                    {
                        string idStr = id.TrimStart(new char[] { 'v' });

                        int idInt;

                        if (int.TryParse(idStr, out idInt) && !addedIds.Contains(idInt))
                        {
                            TwitchVideo video = this.GetTwitchVideoFromId(webClient, "v" + idInt);

                            if (video != null)
                            {
                                videos.Add(video);
                                addedIds.Add(idInt);
                            }
                        }
                    }
                }

                this.Videos = videos;
            }
        }

        private string GetVideoIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            Uri validUrl;

            if (!Uri.TryCreate(url, UriKind.Absolute, out validUrl))
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
                                return "v" + idInt;
                            }
                        }
                    }

                    break;
                }
            }

            return null;
        }

        private TwitchVideo GetTwitchVideoFromId(WebClient webClient, string id)
        {
            if (webClient == null)
            {
                throw new ArgumentNullException(nameof(webClient));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            try
            {
                string result = webClient.DownloadString(string.Format(videoUrl, id));

                JObject videoJson = JObject.Parse(result);

                if (videoJson != null && videoJson.Value<string>("_id").StartsWith("v"))
                {
                    return this.ParseVideo(videoJson);
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse resp = ex.Response as HttpWebResponse;

                if (resp != null && resp.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }

            return null;
        }

        private void LoadVideosRecursive(WebClient webClient, string nextUrl, int limit, int sum, int total, List<JArray> videoArrays)
        {
            string result = webClient.DownloadString(nextUrl);

            JObject videoRequestJson = JObject.Parse(result);

            if (videoRequestJson != null)
            {
                JArray array = videoRequestJson.Value<JArray>("videos");

                videoArrays.Add(array);

                sum += array.Count;

                if (sum < limit && sum < total)
                {
                    JObject linksJson = videoRequestJson.Value<JObject>("_links");

                    string next = linksJson.Value<string>("next");

                    this.LoadVideosRecursive(webClient, next, limit, sum, total, videoArrays);
                }
            }
        }

        public void Enqueue(DownloadParameters downloadParams)
        {
            if (this.paused)
            {
                return;
            }

            lock (this.changeDownloadLockObject)
            {
                this.downloads.Add(new TwitchVideoDownload(downloadParams));
            }
        }

        private void DownloadTimerCallback(object state)
        {
            if (this.paused)
            {
                return;
            }

            this.StartQueuedDownloadIfExists();
        }

        private void StartQueuedDownloadIfExists()
        {
            if (this.paused)
            {
                return;
            }

            if (Monitor.TryEnter(this.changeDownloadLockObject))
            {
                try
                {
                    if (!this.downloads.Where(d => d.DownloadStatus == DownloadStatus.Active).Any())
                    {
                        TwitchVideoDownload download = this.downloads.Where(d => d.DownloadStatus == DownloadStatus.Queued).FirstOrDefault();

                        if (download == null)
                        {
                            return;
                        }

                        DownloadParameters downloadParams = download.DownloadParams;

                        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                        CancellationToken cancellationToken = cancellationTokenSource.Token;

                        string downloadId = download.Id;
                        string urlIdTrimmed = downloadParams.Video.IdTrimmed;
                        string tempDir = Path.Combine(this.preferencesService.CurrentPreferences.DownloadTempFolder, TEMP_PREFIX + downloadId);
                        string playlistFile = Path.Combine(tempDir, PLAYLIST_NAME);
                        string ffmpegFile = Path.Combine(appDir, Environment.Is64BitOperatingSystem ? FFMPEG_EXE_X64 : FFMPEG_EXE_X86);
                        string outputFile = downloadParams.FullPath;

                        bool cropStart = downloadParams.CropStart;
                        bool cropEnd = downloadParams.CropEnd;

                        TimeSpan cropStartTime = downloadParams.CropStartTime;
                        TimeSpan cropEndTime = downloadParams.CropEndTime;

                        TwitchVideoQuality resolution = downloadParams.Resolution;

                        VodAuthInfo vodAuthInfo = downloadParams.VodAuthInfo;

                        Action<DownloadStatus> setDownloadStatus = download.SetDownloadStatus;
                        Action<string> log = download.AppendLog;
                        Action<string> setStatus = download.SetStatus;
                        Action<int> setProgress = download.SetProgress;
                        Action<bool> setIsEncoding = download.SetIsEncoding;

                        Task downloadVideoTask = new Task(() =>
                        {
                            setStatus("Initializing");

                            log("Download task has been started!");

                            this.WriteDownloadInfo(log, downloadParams, ffmpegFile, tempDir);

                            this.CheckTempDirectory(log, tempDir);

                            using (WebClient webClient = this.CreateTwitchWebClient())
                            {
                                cancellationToken.ThrowIfCancellationRequested();

                                string playlistUrl = this.RetrievePlaylistUrlForQuality(log, webClient, resolution, urlIdTrimmed, vodAuthInfo);

                                cancellationToken.ThrowIfCancellationRequested();

                                VodPlaylist vodPlaylist = this.RetrieveVodPlaylist(log, tempDir, playlistUrl);

                                cancellationToken.ThrowIfCancellationRequested();

                                CropInfo cropInfo = this.CropVodPlaylist(vodPlaylist, cropStart, cropEnd, cropStartTime, cropEndTime);

                                cancellationToken.ThrowIfCancellationRequested();

                                this.DownloadParts(log, setStatus, setProgress, vodPlaylist, cancellationToken);

                                cancellationToken.ThrowIfCancellationRequested();

                                this.WriteNewPlaylist(log, vodPlaylist, playlistFile);

                                cancellationToken.ThrowIfCancellationRequested();

                                this.EncodeVideo(log, setStatus, setProgress, setIsEncoding, ffmpegFile, playlistFile, outputFile, cropInfo);
                            }
                        }, cancellationToken);

                        Task continueTask = downloadVideoTask.ContinueWith(task =>
                        {
                            log(Environment.NewLine + Environment.NewLine + "Starting temporary download folder cleanup!");
                            this.CleanUp(tempDir, log);

                            setProgress(100);
                            setIsEncoding(false);

                            bool success = false;

                            if (task.IsFaulted)
                            {
                                setDownloadStatus(DownloadStatus.Error);
                                log(Environment.NewLine + Environment.NewLine + "Download task ended with an error!");

                                if (task.Exception != null)
                                {
                                    log(Environment.NewLine + Environment.NewLine + task.Exception.ToString());
                                }
                            }
                            else if (task.IsCanceled)
                            {
                                setDownloadStatus(DownloadStatus.Canceled);
                                log(Environment.NewLine + Environment.NewLine + "Download task was canceled!");
                            }
                            else
                            {
                                success = true;
                                setDownloadStatus(DownloadStatus.Finished);
                                log(Environment.NewLine + Environment.NewLine + "Download task ended successfully!");
                            }

                            DownloadTask downloadTask;

                            if (!this.downloadTasks.TryRemove(downloadId, out downloadTask))
                            {
                                throw new ApplicationException("Could not remove download task with ID '" + downloadId + "' from download task collection!");
                            }

                            if (success && this.preferencesService.CurrentPreferences.DownloadRemoveCompleted)
                            {
                                this.eventAggregator.GetEvent<RemoveDownloadEvent>().Publish(downloadId);
                            }
                        });

                        if (this.downloadTasks.TryAdd(downloadId, new DownloadTask(downloadVideoTask, continueTask, cancellationTokenSource)))
                        {
                            downloadVideoTask.Start();
                            setDownloadStatus(DownloadStatus.Active);
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(this.changeDownloadLockObject);
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
            log(Environment.NewLine + "Selected Quality: " + downloadParams.Resolution.DisplayStringShort);
            log(Environment.NewLine + "Download Url: " + downloadParams.Video.Url);
            log(Environment.NewLine + "Crop Start: " + (downloadParams.CropStart ? "Yes (" + downloadParams.CropStartTime + ")" : "No"));
            log(Environment.NewLine + "Crop End: " + (downloadParams.CropEnd ? "Yes (" + downloadParams.CropEndTime + ")" : "No"));

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

        private string RetrievePlaylistUrlForQuality(Action<string> log, WebClient webClient, TwitchVideoQuality resolution, string urlIdTrimmed, VodAuthInfo vodAuthInfo)
        {
            log(Environment.NewLine + Environment.NewLine + "Retrieving m3u8 playlist urls for all VOD qualities...");
            string allPlaylistsStr = webClient.DownloadString(string.Format(allPlaylistsUrl, urlIdTrimmed, vodAuthInfo.Signature, vodAuthInfo.Token));
            log(" done!");

            List<string> allPlaylistsList = allPlaylistsStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(s => !s.StartsWith("#")).ToList();

            allPlaylistsList.ForEach(url =>
            {
                log(Environment.NewLine + url);
            });

            string playlistUrl = allPlaylistsList.Where(s => s.ToLowerInvariant().Contains(resolution.QualityId)).First();

            log(Environment.NewLine + Environment.NewLine + "Playlist url for selected quality " + resolution.DisplayStringShort + " is " + playlistUrl);

            return playlistUrl;
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
                        lengthSum += partLength;
                        start = Math.Round(lengthSum - start, 3);
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

        private void EncodeVideo(Action<string> log, Action<string> setStatus, Action<int> setProgress,
            Action<bool> setIsEncoding, string ffmpegFile, string playlistFile, string outputFile, CropInfo cropInfo)
        {
            setStatus("Processing");
            setIsEncoding(true);

            log(Environment.NewLine + Environment.NewLine + "Executing '" + ffmpegFile + "' on local playlist...");

            ProcessStartInfo psi = new ProcessStartInfo(ffmpegFile);
            psi.Arguments = "-y" + " -i \"" + playlistFile + "\" -analyzeduration " + int.MaxValue + " -probesize " + int.MaxValue + " -c:v copy -c:a copy -bsf:a aac_adtstoasc" + (cropInfo.CropStart ? " -ss " + cropInfo.Start.ToString(CultureInfo.InvariantCulture) : null) + (cropInfo.CropEnd ? " -t " + cropInfo.Length.ToString(CultureInfo.InvariantCulture) : null) + " \"" + outputFile + "\"";
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.StandardErrorEncoding = Encoding.UTF8;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            log(Environment.NewLine + "Command line arguments: " + psi.Arguments + Environment.NewLine);

            using (Process p = new Process())
            {
                TimeSpan duration = new TimeSpan();

                bool durationReceived = false;

                DataReceivedEventHandler outputDataReceived = new DataReceivedEventHandler((s, e) =>
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            string dataTrimmed = e.Data.Trim();

                            if (dataTrimmed.StartsWith("Duration") && !durationReceived)
                            {
                                string durationStr = dataTrimmed.Substring(dataTrimmed.IndexOf(":") + 1).Trim();
                                durationStr = durationStr.Substring(0, durationStr.IndexOf(",")).Trim();

                                if (TimeSpan.TryParse(durationStr, out duration))
                                {
                                    duration = TimeSpan.Parse(durationStr);
                                    durationReceived = true;
                                    setProgress(0);
                                }
                            }

                            if (dataTrimmed.StartsWith("frame") && durationReceived && duration != TimeSpan.Zero)
                            {
                                string timeStr = dataTrimmed.Substring(dataTrimmed.IndexOf("time") + 4).Trim();
                                timeStr = timeStr.Substring(timeStr.IndexOf("=") + 1).Trim();
                                timeStr = timeStr.Substring(0, timeStr.IndexOf(" ")).Trim();

                                TimeSpan current;

                                if (TimeSpan.TryParse(timeStr, out current))
                                {
                                    setIsEncoding(false);
                                    setProgress((int)(current.TotalMilliseconds * 100 / duration.TotalMilliseconds));
                                }
                                else
                                {
                                    setIsEncoding(true);
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
                    log(Environment.NewLine + "Encoding complete!");
                }
                else
                {
                    throw new ApplicationException("An error occured while encoding the video!");
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
            lock (this.changeDownloadLockObject)
            {
                DownloadTask downloadTask;

                if (this.downloadTasks.TryGetValue(id, out downloadTask))
                {
                    downloadTask.CancellationTokenSource.Cancel();
                }
            }
        }

        public void Retry(string id)
        {
            if (this.paused)
            {
                return;
            }

            lock (this.changeDownloadLockObject)
            {
                DownloadTask downloadTask;

                if (!this.downloadTasks.TryGetValue(id, out downloadTask))
                {
                    TwitchVideoDownload download = this.downloads.Where(d => d.Id == id).FirstOrDefault();

                    if (download != null && (download.DownloadStatus == DownloadStatus.Canceled || download.DownloadStatus == DownloadStatus.Error))
                    {
                        download.ResetLog();
                        download.SetProgress(0);
                        download.SetDownloadStatus(DownloadStatus.Queued);
                        download.SetStatus("Initializing");
                    }
                }
            }
        }

        public void Remove(string id)
        {
            lock (this.changeDownloadLockObject)
            {
                DownloadTask downloadTask;

                if (!this.downloadTasks.TryGetValue(id, out downloadTask))
                {
                    TwitchVideoDownload download = this.downloads.Where(d => d.Id == id).FirstOrDefault();

                    if (download != null)
                    {
                        this.downloads.Remove(download);
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
            List<TwitchVideoQuality> resolutions = this.ParseResolutions(videoJson.Value<JObject>("resolutions"), videoJson.Value<JObject>("fps"));
            DateTime recordedDate = DateTime.ParseExact(videoJson.Value<string>("recorded_at"), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            Uri thumbnail = new Uri(videoJson.Value<string>("preview"));
            Uri url = new Uri(videoJson.Value<string>("url"));

            return new TwitchVideo(channel, title, id, game, views, length, resolutions, recordedDate, thumbnail, url);
        }

        public List<TwitchVideoQuality> ParseResolutions(JObject resolutionsJson, JObject fpsJson)
        {
            List<TwitchVideoQuality> resolutions = new List<TwitchVideoQuality>();

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
                    string qualityId = resolution.Name;
                    string value = resolution.Value.Value<string>();
                    string fps = fpsList.ContainsKey(qualityId) ? fpsList[qualityId] : null;

                    resolutions.Add(new TwitchVideoQuality(qualityId, value, fps));
                }
            }

            if (fpsList.ContainsKey(TwitchVideoQuality.QUALITY_AUDIO))
            {
                resolutions.Add(new TwitchVideoQuality(TwitchVideoQuality.QUALITY_AUDIO));
            }

            if (!resolutions.Any())
            {
                resolutions.Add(new TwitchVideoQuality(TwitchVideoQuality.QUALITY_SOURCE));
            }

            try
            {
                resolutions = resolutions.OrderBy(r => this.resolutionOrderMap[r.QualityId]).ToList();
            }
            catch
            {
                // If sort fails, ignore
            }

            try
            {
                resolutions = resolutions.OrderBy(r => this.qualityOrderMap[r.QualityId]).ToList();
            }
            catch
            {
                // If sort fails, ignore
            }

            return resolutions;
        }

        public void Pause()
        {
            this.paused = true;
            this.downloadTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Resume()
        {
            this.paused = false;
            this.downloadTimer.Change(0, TIMER_INTERVALL);
        }

        public bool CanShutdown()
        {
            Monitor.Enter(this.changeDownloadLockObject);

            try
            {
                return !this.downloads.Where(d => d.DownloadStatus == DownloadStatus.Active || d.DownloadStatus == DownloadStatus.Queued).Any();
            }
            finally
            {
                Monitor.Exit(this.changeDownloadLockObject);
            }
        }

        public void Shutdown()
        {
            this.Pause();

            foreach (DownloadTask downloadTask in this.downloadTasks.Values)
            {
                downloadTask.CancellationTokenSource.Cancel();
            }

            List<Task> tasks = this.downloadTasks.Values.Select(v => v.Task).ToList();
            tasks.AddRange(this.downloadTasks.Values.Select(v => v.ContinueTask).ToList());

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception)
            {
                // Don't care about aborted tasks
            }

            List<string> toRemove = this.downloads.Select(d => d.Id).ToList();

            foreach (string id in toRemove)
            {
                this.Remove(id);
            }
        }

        public bool IsFileNameUsed(string fullPath)
        {
            IEnumerable<TwitchVideoDownload> downloads = this.downloads.Where(d => d.DownloadStatus == DownloadStatus.Active || d.DownloadStatus == DownloadStatus.Queued);

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
            this.runtimeDataService.RuntimeData.AccessToken = this.twitchAuthInfo != null ? this.twitchAuthInfo.AccessToken : null;
            this.runtimeDataService.Save();

            this.FirePropertyChanged(nameof(this.IsAuthorized));
            this.eventAggregator.GetEvent<IsAuthorizedChangedEvent>().Publish(this.IsAuthorized);
        }

        private void FireVideosCountChanged()
        {
            this.eventAggregator.GetEvent<VideosCountChangedEvent>().Publish(this.videos != null ? this.videos.Count : 0);
        }

        private void FireDownloadsCountChanged()
        {
            this.eventAggregator.GetEvent<DownloadsCountChangedEvent>().Publish(this.downloads != null ? this.downloads.Count : 0);
        }

        #endregion Methods

        #region EventHandlers

        private void Videos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.FireVideosCountChanged();
        }

        private void Downloads_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.FireDownloadsCountChanged();
        }

        #endregion EventHandlers
    }
}