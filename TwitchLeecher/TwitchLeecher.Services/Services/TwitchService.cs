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
using TwitchLeecher.Services.Extensions;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Services.Models;
using TwitchLeecher.Shared.Events;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Services.Services
{
    internal class TwitchService : BindableBase, ITwitchService
    {
        #region Constants

        private const string krakenUrl = "https://api.twitch.tv/kraken";
        private const string channelsUrl = "https://api.twitch.tv/kraken/channels/{0}";
        private const string videosUrl = "https://api.twitch.tv/kraken/channels/{0}/videos";
        private const string accessTokenUrl = "https://api.twitch.tv/api/vods/{0}/access_token";

        private const string allPlaylistsUrl = "https://usher.twitch.tv/vod/{0}?nauthsig={1}&nauth={2}&allow_source=true&player=twitchweb&allow_spectre=true&allow_audio_only=true";

        private const string TEMP_PREFIX = "TL_";
        private const string PLAYLIST_NAME = "vod.m3u8";
        private const string FFMPEG_EXE_X86 = "ffmpeg_x86.exe";
        private const string FFMPEG_EXE_X64 = "ffmpeg_x64.exe";

        private const int TIMER_INTERVALL = 2;

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
        private Dictionary<VideoQuality, int> orderMap;
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

            this.orderMap = new Dictionary<VideoQuality, int>();
            this.orderMap.Add(VideoQuality.Source, 0);
            this.orderMap.Add(VideoQuality.High, 1);
            this.orderMap.Add(VideoQuality.Medium, 2);
            this.orderMap.Add(VideoQuality.Low, 3);
            this.orderMap.Add(VideoQuality.Mobile, 4);
            this.orderMap.Add(VideoQuality.AudioOnly, 5);

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
            using (WebClient webClient = this.CreateTwitchWebClient())
            {
                ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

                if (searchParams.VideoType == VideoType.Broadcast)
                {
                    webClient.QueryString.Add("broadcasts", "true");
                }

                int limit = searchParams.LoadLimit;

                webClient.QueryString.Add("limit", (limit > TWITCH_MAX_LOAD_LIMIT ? TWITCH_MAX_LOAD_LIMIT.ToString() : limit.ToString()));

                string result = webClient.DownloadString(string.Format(videosUrl, searchParams.Username));

                JObject videoRequestJson = JObject.Parse(result);

                if (videoRequestJson != null)
                {
                    List<JArray> videoArrays = new List<JArray>();

                    JArray firstArray = videoRequestJson.Value<JArray>("videos");

                    videoArrays.Add(firstArray);

                    int sum = firstArray.Count;
                    int total = videoRequestJson.Value<int>("_total");

                    if (limit > TWITCH_MAX_LOAD_LIMIT && sum < total)
                    {
                        JObject linksJson = videoRequestJson.Value<JObject>("_links");

                        string nextUrl = linksJson.Value<string>("next");

                        webClient.QueryString.Clear();

                        this.LoadVideosRecursive(webClient, nextUrl, limit, sum, total, videoArrays);
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

                        TwitchVideoResolution resolution = downloadParams.Resolution;

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

                                WebChunkList webChunkList = this.RetrieveWebChunkList(log, tempDir, playlistUrl);

                                cancellationToken.ThrowIfCancellationRequested();

                                CropInfo cropInfo = this.CropWebChunkList(webChunkList, cropStart, cropEnd, cropStartTime, cropEndTime);

                                cancellationToken.ThrowIfCancellationRequested();

                                this.DownloadChunks(log, setStatus, setProgress, webChunkList, cancellationToken);

                                cancellationToken.ThrowIfCancellationRequested();

                                this.WriteNewPlaylist(log, webChunkList, playlistFile);

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
            log(Environment.NewLine + Environment.NewLine + "VOD INFO");
            log(Environment.NewLine + "--------------------------------------------------------------------------------------------");
            log(Environment.NewLine + "VOD ID: " + downloadParams.Video.IdTrimmed);
            log(Environment.NewLine + "Selected Quality: " + downloadParams.Resolution.ResolutionAsString);
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

        private string RetrievePlaylistUrlForQuality(Action<string> log, WebClient webClient, TwitchVideoResolution resolution, string urlIdTrimmed, VodAuthInfo vodAuthInfo)
        {
            log(Environment.NewLine + Environment.NewLine + "Retrieving m3u8 playlist urls for all VOD qualities...");
            string allPlaylistsStr = webClient.DownloadString(string.Format(allPlaylistsUrl, urlIdTrimmed, vodAuthInfo.Signature, vodAuthInfo.Token));
            log(" done!");

            List<string> allPlaylistsList = allPlaylistsStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(s => !s.StartsWith("#")).ToList();

            allPlaylistsList.ForEach(url =>
            {
                log(Environment.NewLine + url);
            });

            string playlistUrl = allPlaylistsList.Where(s => s.ToLowerInvariant().Contains(resolution.VideoQuality.ToTwitchQuality())).First();

            log(Environment.NewLine + Environment.NewLine + "Playlist url for selected quality " + resolution.ResolutionAsString + " is " + playlistUrl);

            return playlistUrl;
        }

        private WebChunkList RetrieveWebChunkList(Action<string> log, string tempDir, string playlistUrl)
        {
            using (WebClient webClient = new WebClient())
            {
                log(Environment.NewLine + Environment.NewLine + "Retrieving playlist...");
                string playlistStr = webClient.DownloadString(playlistUrl);
                log(" done!");

                string playlistUrlPrefix = playlistUrl.Substring(0, playlistUrl.LastIndexOf("/") + 1);

                log(Environment.NewLine + "Parsing playlist...");
                WebChunkList webChunkList = WebChunkList.Parse(tempDir, playlistStr, playlistUrlPrefix);
                log(" done!");

                log(Environment.NewLine + "Number of video chunks: " + webChunkList.Content.Count);

                return webChunkList;
            }
        }

        private CropInfo CropWebChunkList(WebChunkList webChunkList, bool cropStart, bool cropEnd, TimeSpan cropStartTime, TimeSpan cropEndTime)
        {
            double start = cropStartTime.TotalMilliseconds;
            double lengthOrg = cropEndTime.TotalMilliseconds;
            double length = cropEndTime.TotalMilliseconds;

            if (cropStart)
            {
                length -= start;
            }

            start = Math.Round(start / 1000, 3);
            lengthOrg = Math.Round(lengthOrg / 1000, 3);
            length = Math.Round(length / 1000, 3);

            List<WebChunk> content = webChunkList.Content;

            List<WebChunk> deleteStart = new List<WebChunk>();
            List<WebChunk> deleteEnd = new List<WebChunk>();

            if (cropStart)
            {
                double chunkSum = 0;

                foreach (WebChunk webChunk in content)
                {
                    chunkSum += webChunk.Length;

                    if (chunkSum < start)
                    {
                        deleteStart.Add(webChunk);
                    }
                    else
                    {
                        start = Math.Round(chunkSum - start, 3);
                        break;
                    }
                }
            }

            if (cropEnd)
            {
                double chunkSum = 0;

                foreach (WebChunk webChunk in content)
                {
                    chunkSum += webChunk.Length;

                    if (chunkSum > lengthOrg)
                    {
                        deleteEnd.Add(webChunk);
                    }
                }
            }

            deleteStart.ForEach(webChunk =>
            {
                content.Remove(webChunk);
            });

            deleteEnd.ForEach(webChunk =>
            {
                content.Remove(webChunk);
            });

            return new CropInfo(cropStart, cropEnd, cropStart ? start : 0, length);
        }

        private void DownloadChunks(Action<string> log, Action<string> setStatus, Action<int> setProgress,
            WebChunkList webChunkList, CancellationToken cancellationToken)
        {
            int webChunkCount = webChunkList.Content.Count;
            int maxConnectionCount = ServicePointManager.DefaultConnectionLimit;

            log(Environment.NewLine + Environment.NewLine + "Starting parallel video chunk download");
            log(Environment.NewLine + "Number of video chunks to download: " + webChunkCount);
            log(Environment.NewLine + "Maximum connection count: " + maxConnectionCount);

            setStatus("Downloading");

            log(Environment.NewLine + Environment.NewLine + "Parallel video chunk download is running...");

            long completedChunkDownloads = 0;

            Parallel.ForEach(webChunkList.Content, new ParallelOptions() { MaxDegreeOfParallelism = maxConnectionCount - 1 }, (webChunk, loopState) =>
            {
                using (WebClient downloadClient = new WebClient())
                {
                    byte[] bytes = downloadClient.DownloadData(webChunk.DownloadUrl);

                    Interlocked.Increment(ref completedChunkDownloads);

                    FileSystem.DeleteFile(webChunk.LocalFile);

                    File.WriteAllBytes(webChunk.LocalFile, bytes);

                    long completed = Interlocked.Read(ref completedChunkDownloads);

                    setProgress((int)(completedChunkDownloads * 100 / webChunkCount));
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    loopState.Stop();
                }
            });

            setProgress(100);

            log(" done!");

            log(Environment.NewLine + Environment.NewLine + "Download of all video chunks complete!");
        }

        private void WriteNewPlaylist(Action<string> log, WebChunkList webChunkList, string playlistFile)
        {
            log(Environment.NewLine + Environment.NewLine + "Creating local m3u8 playlist for FFMPEG...");

            StringBuilder sb = new StringBuilder();

            webChunkList.Header.ForEach(line =>
            {
                sb.AppendLine(line);
            });

            webChunkList.Content.ForEach(webChunk =>
            {
                sb.AppendLine(webChunk.ExtInf);
                sb.AppendLine(webChunk.LocalFile);
            });

            webChunkList.Footer.ForEach(line =>
            {
                sb.AppendLine(line);
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
            setStatus("Encoding");
            setIsEncoding(true);

            log(Environment.NewLine + Environment.NewLine + "Executing '" + ffmpegFile + "' on local playlist...");

            ProcessStartInfo psi = new ProcessStartInfo(ffmpegFile);
            psi.Arguments = "-y" + (cropInfo.CropStart ? " -ss " + cropInfo.Start.ToString(CultureInfo.InvariantCulture) : null) + " -i \"" + playlistFile + "\" -c:v copy -c:a copy -bsf:a aac_adtstoasc" + (cropInfo.CropEnd ? " -t " + cropInfo.Length.ToString(CultureInfo.InvariantCulture) : null) + " \"" + outputFile + "\"";
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
            List<TwitchVideoResolution> resolutions = this.ParseResolutions(videoJson.Value<JObject>("resolutions"), videoJson.Value<JObject>("fps"));
            DateTime recordedDate = DateTime.ParseExact(videoJson.Value<string>("recorded_at"), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            Uri thumbnail = new Uri(videoJson.Value<string>("preview"));
            Uri url = new Uri(videoJson.Value<string>("url"));

            return new TwitchVideo(channel, title, id, game, views, length, resolutions, recordedDate, thumbnail, url);
        }

        public List<TwitchVideoResolution> ParseResolutions(JObject resolutionsJson, JObject fpsJson)
        {
            List<TwitchVideoResolution> resolutions = new List<TwitchVideoResolution>();

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
                    string quality = resolution.Name;
                    string value = resolution.Value.Value<string>();

                    switch (quality)
                    {
                        case "chunked":
                            resolutions.Add(new TwitchVideoResolution(VideoQuality.Source, value, fpsList.ContainsKey("chunked") ? fpsList["chunked"] : null));
                            break;

                        case "high":
                            resolutions.Add(new TwitchVideoResolution(VideoQuality.High, value, fpsList.ContainsKey("high") ? fpsList["high"] : null));
                            break;

                        case "medium":
                            resolutions.Add(new TwitchVideoResolution(VideoQuality.Medium, value, fpsList.ContainsKey("medium") ? fpsList["medium"] : null));
                            break;

                        case "low":
                            resolutions.Add(new TwitchVideoResolution(VideoQuality.Low, value, fpsList.ContainsKey("low") ? fpsList["low"] : null));
                            break;

                        case "mobile":
                            resolutions.Add(new TwitchVideoResolution(VideoQuality.Mobile, value, fpsList.ContainsKey("mobile") ? fpsList["mobile"] : null));
                            break;
                    }
                }
            }

            if (fpsList.ContainsKey("audio_only"))
            {
                resolutions.Add(new TwitchVideoResolution(VideoQuality.AudioOnly, null, null));
            }

            if (!resolutions.Any())
            {
                resolutions.Add(new TwitchVideoResolution(VideoQuality.Source, null, null));
            }

            resolutions = resolutions.OrderBy(r => this.orderMap[r.VideoQuality]).ToList();

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

        private VideoQuality ConvertStringToVideoQuality(string quality)
        {
            if (string.IsNullOrEmpty(quality))
            {
                throw new ArgumentNullException(nameof(quality));
            }

            switch (quality)
            {
                case "chunked":
                    return VideoQuality.Source;

                case "high":
                    return VideoQuality.High;

                case "medium":
                    return VideoQuality.Medium;

                case "low":
                    return VideoQuality.Low;

                case "mobile":
                    return VideoQuality.Mobile;

                case "audio_only":
                    return VideoQuality.AudioOnly;

                default:
                    throw new ApplicationException("Cannot convert '" + quality + "' into type '" + typeof(VideoQuality).FullName + "'!");
            }
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