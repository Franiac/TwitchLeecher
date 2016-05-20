using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        private const string usersUrl = "https://api.twitch.tv/kraken/users/{0}";
        private const string videosUrl = "https://api.twitch.tv/kraken/channels/{0}/videos";
        private const string accessTokenUrl = "https://api.twitch.tv/api/vods/{0}/access_token";
        private const string allPlaylistsUrl = "https://usher.twitch.tv/vod/{0}?nauthsig={1}&nauth={2}&allow_source=true&player=twitchweb&allow_spectre=true";

        private const string TEMP_PREFIX = "TL_";
        private const string PLAYLIST_NAME = "vod.m3u8";
        private const string FFMPEG_EXE_X86 = "ffmpeg_x86.exe";
        private const string FFMPEG_EXE_X64 = "ffmpeg_x64.exe";

        private const int TIMER_INTERVALL = 2;

        #endregion Constants

        #region Fields

        private IPreferencesService preferencesService;
        private IEventAggregator eventAggregator;

        private Timer downloadTimer;

        private ObservableCollection<TwitchVideo> videos;
        private ObservableCollection<TwitchVideoDownload> downloads;

        private ConcurrentDictionary<string, DownloadTask> downloadTasks;

        private string appDir;

        private object changeDownloadLockObject;

        private volatile bool paused;

        private Dictionary<VideoQuality, int> orderMap;

        #endregion Fields

        #region Constructors

        public TwitchService(IPreferencesService preferencesService, IEventAggregator eventAggregator)
        {
            if (preferencesService == null)
            {
                throw new ArgumentNullException(nameof(preferencesService));
            }

            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.preferencesService = preferencesService;
            this.eventAggregator = eventAggregator;

            this.videos = new ObservableCollection<TwitchVideo>();
            this.downloads = new ObservableCollection<TwitchVideoDownload>();

            this.downloadTasks = new ConcurrentDictionary<string, DownloadTask>();

            this.appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            this.changeDownloadLockObject = new object();

            this.orderMap = new Dictionary<VideoQuality, int>();
            this.orderMap.Add(VideoQuality.Source, 0);
            this.orderMap.Add(VideoQuality.High, 1);
            this.orderMap.Add(VideoQuality.Medium, 2);
            this.orderMap.Add(VideoQuality.Low, 3);
            this.orderMap.Add(VideoQuality.Mobile, 4);

            this.downloadTimer = new Timer(this.DownloadTimerCallback, null, 0, TIMER_INTERVALL);
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<TwitchVideo> Videos
        {
            get
            {
                return this.videos;
            }
            private set
            {
                this.SetProperty(ref this.videos, value, nameof(this.Videos));
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
                this.SetProperty(ref this.downloads, value, nameof(this.Downloads));
            }
        }

        #endregion Properties

        #region Methods

        public bool UserExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            using (WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 })
            {
                try
                {
                    string result = webClient.DownloadString(string.Format(usersUrl, username));
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void Search(SearchParameters searchParams)
        {
            using (WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 })
            {
                if (searchParams.VideoType == VideoType.Broadcast)
                {
                    webClient.QueryString.Add("broadcasts", "true");
                }

                webClient.QueryString.Add("limit", searchParams.LoadLimit.ToString());

                string result = webClient.DownloadString(string.Format(videosUrl, searchParams.Username));

                JObject videoListJson = JObject.Parse(result);

                ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

                if (videoListJson != null)
                {
                    foreach (JObject videoJson in videoListJson.Value<JArray>("videos"))
                    {
                        if (videoJson.Value<string>("_id").StartsWith("v"))
                        {
                            videos.Add(this.ParseVideo(videoJson));
                        }
                    }
                }

                this.Videos = videos;
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

                        string urlId = downloadParams.Video.Id;
                        string urlIdTrimmed = downloadParams.Video.IdTrimmed;
                        string tempDir = Path.Combine(downloadParams.Folder, TEMP_PREFIX + urlIdTrimmed);
                        string playlistFile = Path.Combine(tempDir, PLAYLIST_NAME);
                        string ffmpegFile = Path.Combine(appDir, Environment.Is64BitOperatingSystem ? FFMPEG_EXE_X64 : FFMPEG_EXE_X86);
                        string outputFile = downloadParams.FullPath;

                        bool cropStart = downloadParams.CropStart;
                        bool cropEnd = downloadParams.CropEnd;

                        TimeSpan cropStartTime = downloadParams.CropStartTime;
                        TimeSpan cropEndTime = downloadParams.CropEndTime;

                        TwitchVideoResolution resolution = downloadParams.Resolution;

                        Action<DownloadStatus> setDownloadStatus = download.SetDownloadStatus;
                        Action<string> log = download.AppendLog;
                        Action<string> setStatus = download.SetStatus;
                        Action<int> setProgress = download.SetProgress;

                        Task downloadVideoTask = new Task(() =>
                        {
                            setStatus("Initializing");

                            log("Download task has been started!");

                            this.WriteDownloadInfo(log, downloadParams, ffmpegFile, tempDir);

                            this.CheckTempDirectory(log, tempDir);

                            using (WebClient webClient = new WebClient())
                            {
                                AuthInfo authInfo = this.RetrieveAuthInfo(log, webClient, urlIdTrimmed);

                                cancellationToken.ThrowIfCancellationRequested();

                                string playlistUrl = this.RetrievePlaylistUrlForQuality(log, webClient, resolution, urlIdTrimmed, authInfo);

                                cancellationToken.ThrowIfCancellationRequested();

                                WebChunkList webChunkList = this.RetrieveWebChunkList(log, webClient, tempDir, playlistUrl);

                                cancellationToken.ThrowIfCancellationRequested();

                                CropInfo cropInfo = this.CropWebChunkList(webChunkList, cropStart, cropEnd, cropStartTime, cropEndTime);

                                cancellationToken.ThrowIfCancellationRequested();

                                this.DownloadChunks(log, setStatus, setProgress, webChunkList, cancellationToken);

                                cancellationToken.ThrowIfCancellationRequested();

                                this.WriteNewPlaylist(log, webChunkList, playlistFile);

                                cancellationToken.ThrowIfCancellationRequested();

                                this.EncodeVideo(log, setStatus, setProgress, ffmpegFile, playlistFile, outputFile, cropInfo);
                            }
                        }, cancellationToken);

                        Task continueTask = downloadVideoTask.ContinueWith(task =>
                        {
                            log(Environment.NewLine + Environment.NewLine + "Starting temporary download folder cleanup!");
                            this.CleanUp(tempDir, log);

                            setProgress(100);

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

                            if (!this.downloadTasks.TryRemove(urlId, out downloadTask))
                            {
                                throw new ApplicationException("Could not remove download task with ID '" + urlId + "' from download task collection!");
                            }

                            if (success && this.preferencesService.CurrentPreferences.DownloadRemoveCompleted)
                            {
                                this.eventAggregator.GetEvent<DownloadCompletedEvent>().Publish(urlId);
                            }
                        });

                        if (this.downloadTasks.TryAdd(urlId, new DownloadTask(downloadVideoTask, continueTask, cancellationTokenSource)))
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
            log(Environment.NewLine + Environment.NewLine + "VOD ID: " + downloadParams.Video.IdTrimmed);
            log(Environment.NewLine + "Selected Quality: " + downloadParams.Resolution.ResolutionFps);
            log(Environment.NewLine + "Download Url: " + downloadParams.Video.Url);
            log(Environment.NewLine + "Output File: " + downloadParams.FullPath);
            log(Environment.NewLine + "FFMPEG Path: " + ffmpegFile);
            log(Environment.NewLine + "Temporary Download Folder: " + tempDir);
            log(Environment.NewLine + "Crop Start: " + (downloadParams.CropStart ? "Yes (" + downloadParams.CropStartTime + ")" : "No"));
            log(Environment.NewLine + "Crop End: " + (downloadParams.CropEnd ? "Yes (" + downloadParams.CropEndTime + ")" : "No"));
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

        private AuthInfo RetrieveAuthInfo(Action<string> log, WebClient webClient, string urlIdTrimmed)
        {
            log(Environment.NewLine + Environment.NewLine + "Retrieving access token and signature...");
            string accessTokenStr = webClient.DownloadString(string.Format(accessTokenUrl, urlIdTrimmed));
            log(" done!");

            JObject accessTokenJson = JObject.Parse(accessTokenStr);

            string token = Uri.EscapeDataString(accessTokenJson.Value<string>("token"));
            string signature = accessTokenJson.Value<string>("sig");

            log(Environment.NewLine + "Token: " + token);
            log(Environment.NewLine + "Signature: " + signature);

            return new AuthInfo(token, signature);
        }

        private string RetrievePlaylistUrlForQuality(Action<string> log, WebClient webClient, TwitchVideoResolution resolution, string urlIdTrimmed, AuthInfo authInfo)
        {
            log(Environment.NewLine + Environment.NewLine + "Retrieving m3u8 playlist urls for all VOD qualities...");
            string allPlaylistsStr = webClient.DownloadString(string.Format(allPlaylistsUrl, urlIdTrimmed, authInfo.Signature, authInfo.Token));
            log(" done!");

            List<string> allPlaylistsList = allPlaylistsStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(s => !s.StartsWith("#")).ToList();

            allPlaylistsList.ForEach(url =>
            {
                log(Environment.NewLine + url);
            });

            string playlistUrl = allPlaylistsList.Where(s => s.ToLowerInvariant().Contains(resolution.VideoQuality.ToTwitchQuality())).First();

            log(Environment.NewLine + Environment.NewLine + "Playlist url for selected quality " + resolution.ResolutionFps + " is " + playlistUrl);

            return playlistUrl;
        }

        private WebChunkList RetrieveWebChunkList(Action<string> log, WebClient webClient, string tempDir, string playlistUrl)
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
            string ffmpegFile, string playlistFile, string outputFile, CropInfo cropInfo)
        {
            setStatus("Encoding");

            log(Environment.NewLine + Environment.NewLine + "Executing '" + ffmpegFile + "' on local playlist...");

            ProcessStartInfo psi = new ProcessStartInfo(ffmpegFile);
            psi.Arguments = "-y" + (cropInfo.CropStart ? " -ss " + cropInfo.Start.ToString(CultureInfo.InvariantCulture) : null) + " -i \"" + playlistFile + "\" -c:v copy -c:a copy -bsf:a aac_adtstoasc" + (cropInfo.CropEnd ? " -loglevel error -t " + cropInfo.Length.ToString(CultureInfo.InvariantCulture) : null) + " \"" + outputFile + "\"";
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.StandardErrorEncoding = Encoding.UTF8;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            log(Environment.NewLine + "Command line arguments: " + psi.Arguments + Environment.NewLine);

            using (Process p = new Process())
            {
                DataReceivedEventHandler outputDataReceived = new DataReceivedEventHandler((s, e) =>
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(e.Data))
                        {
                            log(Environment.NewLine + e.Data.Trim());
                        }
                    }
                    catch (Exception ex)
                    {
                        log(Environment.NewLine + "An error occured while reading '" + ffmpegFile + "' output stream!" + Environment.NewLine + Environment.NewLine + ex.ToString());
                        p.Kill();
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
                    TwitchVideoDownload download = this.downloads.Where(d => d.DownloadParams.Video.Id == id).FirstOrDefault();

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
                    TwitchVideoDownload download = this.downloads.Where(d => d.DownloadParams.Video.Id == id).FirstOrDefault();

                    if (download != null)
                    {
                        this.downloads.Remove(download);
                    }
                }
            }
        }

        public TwitchVideo ParseVideo(JObject videoJson)
        {
            string title = videoJson.Value<string>("title");
            string id = videoJson.Value<string>("_id");
            string game = videoJson.Value<string>("game");
            int views = videoJson.Value<int>("views");
            TimeSpan length = new TimeSpan(0, 0, videoJson.Value<int>("length"));
            List<TwitchVideoResolution> resolutions = this.ParseResolutions(videoJson.Value<JObject>("resolutions"), videoJson.Value<JObject>("fps"));
            DateTime recordedDate = DateTime.ParseExact(videoJson.Value<string>("recorded_at"), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            Uri thumbnail = new Uri(videoJson.Value<string>("preview"));
            Uri url = new Uri(videoJson.Value<string>("url"));

            return new TwitchVideo(title, id, game, views, length, resolutions, recordedDate, thumbnail, url);
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

            List<string> toRemove = this.downloads.Select(d => d.DownloadParams.Video.Id).ToList();

            foreach (string id in toRemove)
            {
                this.Remove(id);
            }
        }

        #endregion Methods
    }
}