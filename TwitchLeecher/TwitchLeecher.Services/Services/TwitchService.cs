using Newtonsoft.Json;
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
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Extensions;
using TwitchLeecher.Services.Interfaces;
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

        private const string DOWNLOADS_FOLDER_NAME = "Downloads";
        private const string FFMPEG_EXE_X86 = "ffmpeg_x86.exe";
        private const string FFMPEG_EXE_X64 = "ffmpeg_x64.exe";

        private const int TIMER_INTERVALL = 2;

        #endregion Constants

        #region Fields

        private Timer downloadTimer;

        private ObservableCollection<TwitchVideo> videos;
        private ObservableCollection<TwitchVideoDownload> downloads;

        private ConcurrentDictionary<string, DownloadTask> downloadTasks;

        private string appDir;
        private string downloadsDir;

        private object changeDownloadLockObject;

        private volatile bool paused;

        #endregion Fields

        #region Constructors

        public TwitchService()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };
            ServicePointManager.DefaultConnectionLimit = 10;

            this.videos = new ObservableCollection<TwitchVideo>();
            this.downloads = new ObservableCollection<TwitchVideoDownload>();

            this.downloadTasks = new ConcurrentDictionary<string, DownloadTask>();

            this.appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.downloadsDir = Path.Combine(this.appDir, DOWNLOADS_FOLDER_NAME);

            this.changeDownloadLockObject = new object();

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

                dynamic videoListJson = JsonConvert.DeserializeObject(result);

                ObservableCollection<TwitchVideo> videos = new ObservableCollection<TwitchVideo>();

                if (videoListJson != null)
                {
                    foreach (dynamic videoJson in videoListJson.videos)
                    {
                        if (videoJson._id.ToString().StartsWith("v"))
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
                this.Downloads.Add(new TwitchVideoDownload(downloadParams));
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
                    if (!this.Downloads.Where(d => d.DownloadStatus == DownloadStatus.Active).Any())
                    {
                        TwitchVideoDownload download = this.Downloads.Where(d => d.DownloadStatus == DownloadStatus.Queued).FirstOrDefault();

                        if (download != null)
                        {
                            TwitchVideo video = download.Video;
                            DownloadParameters downloadParams = download.DownloadParams;

                            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                            CancellationToken cancellationToken = cancellationTokenSource.Token;

                            long completedChunkDownloads = 0;

                            int maxConnectionCount = ServicePointManager.DefaultConnectionLimit;

                            string urlId = video.Id.Substring(1);
                            string quality = downloadParams.Resolution.VideoQuality.ToTwitchQuality();
                            string qualityFps = downloadParams.Resolution.ResolutionFps;
                            string outputDir = Path.Combine(downloadsDir, urlId);
                            string playlistFile = Path.Combine(outputDir, "vod.m3u8");
                            string ffmpegFile = Path.Combine(appDir, Environment.Is64BitOperatingSystem ? FFMPEG_EXE_X64 : FFMPEG_EXE_X86);
                            string outputFile = downloadParams.Filename;

                            Task downloadVideoTask = new Task(() =>
                            {
                                download.Status = "Initializing";

                                download.AppendLog("Download task has been started!");

                                download.AppendLog(Environment.NewLine + Environment.NewLine + "VOD ID: " + urlId);
                                download.AppendLog(Environment.NewLine + "Selected Quality: " + qualityFps);
                                download.AppendLog(Environment.NewLine + "Download Url: " + video.Url.ToString());
                                download.AppendLog(Environment.NewLine + "Output File: " + outputFile);
                                download.AppendLog(Environment.NewLine + "FFMPEG Path: " + ffmpegFile);
                                download.AppendLog(Environment.NewLine + "Temporary Download Folder: " + outputDir);

                                if (!Directory.Exists(outputDir))
                                {
                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Creating directory '" + outputDir + "'...");
                                    FileSystem.CreateDirectory(outputDir);
                                    download.AppendLog(" done!");
                                }

                                if (Directory.EnumerateFileSystemEntries(outputDir).Any())
                                {
                                    throw new ApplicationException("Temporary download directory '" + outputDir + "' is not empty!");
                                }

                                using (WebClient webClient = new WebClient())
                                {
                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Retrieving access token and signature...");
                                    string accessTokenStr = webClient.DownloadString(string.Format(accessTokenUrl, urlId));
                                    download.AppendLog(" done!");

                                    dynamic accessTokenJson = JsonConvert.DeserializeObject(accessTokenStr);

                                    string token = Uri.EscapeDataString(accessTokenJson.token.ToString());
                                    string sig = accessTokenJson.sig.ToString();

                                    download.AppendLog(Environment.NewLine + "Token: " + token);
                                    download.AppendLog(Environment.NewLine + "Signature: " + sig);

                                    cancellationToken.ThrowIfCancellationRequested();

                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Retrieving m3u8 playlist urls for all VOD qualities...");
                                    string allPlaylistsStr = webClient.DownloadString(string.Format(allPlaylistsUrl, urlId, sig, token));
                                    download.AppendLog(" done!");

                                    List<string> allPlaylistsList = allPlaylistsStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(s => !s.StartsWith("#")).ToList();

                                    allPlaylistsList.ForEach(url =>
                                    {
                                        download.AppendLog(Environment.NewLine + url);
                                    });

                                    string playlistUrl = allPlaylistsList.Where(s => s.ToLowerInvariant().Contains(quality)).First();

                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Playlist url for selected quality " + qualityFps + " is " + playlistUrl);

                                    cancellationToken.ThrowIfCancellationRequested();

                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Retrieving list of video chunks...");
                                    string playlistStr = webClient.DownloadString(playlistUrl);
                                    download.AppendLog(" done!");

                                    List<string> playlistLines = playlistStr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                                    List<string> webChunkList = playlistLines.Where(s => !s.StartsWith("#")).ToList();

                                    string webChunkUrlPrefix = playlistUrl.Substring(0, playlistUrl.LastIndexOf("/") + 1);

                                    long webChunkCount = webChunkList.Count;
                                    download.AppendLog(Environment.NewLine + "Number of video chunks to download: " + webChunkCount);
                                    download.AppendLog(Environment.NewLine + "Maximum connection count: " + maxConnectionCount);

                                    List<string> webChunkFilenames = new List<string>();

                                    List<WebChunk> downloadQueue = new List<WebChunk>();

                                    int counter = 0;

                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Initializing video chunk download queue...");

                                    foreach (string webChunk in webChunkList)
                                    {
                                        string filename = Path.Combine(outputDir, counter.ToString("D8") + ".ts");
                                        webChunkFilenames.Add(filename);
                                        downloadQueue.Add(new WebChunk(filename, webChunkUrlPrefix + webChunk));
                                        counter++;

                                        cancellationToken.ThrowIfCancellationRequested();
                                    }

                                    download.AppendLog(" done!");

                                    download.AppendLog(Environment.NewLine + "Starting parallel video chunk download...");

                                    download.Status = "Downloading";

                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Parallel video chunk download is running...");

                                    object percentageLock = new object();

                                    Parallel.ForEach(downloadQueue, new ParallelOptions() { MaxDegreeOfParallelism = maxConnectionCount - 1 }, (webChunk, loopState) =>
                                    {
                                        using (WebClient downloadClient = new WebClient())
                                        {
                                            byte[] bytes = downloadClient.DownloadData(webChunk.Url);

                                            Interlocked.Increment(ref completedChunkDownloads);

                                            FileSystem.DeleteFile(webChunk.Filename);

                                            File.WriteAllBytes(webChunk.Filename, bytes);

                                            long completed = Interlocked.Read(ref completedChunkDownloads);

                                            lock (percentageLock)
                                            {
                                                download.Progress = (int)(completedChunkDownloads * 100 / webChunkCount);
                                            }
                                        }

                                        if (cancellationToken.IsCancellationRequested)
                                        {
                                            loopState.Stop();
                                        }
                                    });

                                    download.Progress = 100;

                                    cancellationToken.ThrowIfCancellationRequested();

                                    download.AppendLog(" done!");

                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Download of all video chunks complete!");

                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Creating local m3u8 playlist for FFMPEG");

                                    int chunkIndex = 0;

                                    for (int i = 0; i < playlistLines.Count; i++)
                                    {
                                        if (!playlistLines[i].StartsWith("#"))
                                        {
                                            playlistLines[i] = webChunkFilenames[chunkIndex];
                                            chunkIndex++;
                                        }
                                    }

                                    string newPlaylistStr = string.Join("\n", playlistLines);

                                    FileSystem.DeleteFile(playlistFile);

                                    download.AppendLog(Environment.NewLine + "Writing playlist to '" + playlistFile + "'");
                                    File.WriteAllText(playlistFile, newPlaylistStr);

                                    download.Status = "Encoding";

                                    download.Progress = 0;

                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Executing '" + ffmpegFile + "' on local playlist...");

                                    cancellationToken.ThrowIfCancellationRequested();

                                    ProcessStartInfo psi = new ProcessStartInfo(ffmpegFile);
                                    psi.Arguments = "-y -i \"" + playlistFile + "\" -c:v copy -c:a copy -bsf:a aac_adtstoasc \"" + outputFile + "\"";
                                    psi.RedirectStandardError = true;
                                    psi.RedirectStandardOutput = true;
                                    psi.StandardErrorEncoding = Encoding.UTF8;
                                    psi.StandardOutputEncoding = Encoding.UTF8;
                                    psi.UseShellExecute = false;
                                    psi.CreateNoWindow = true;

                                    download.AppendLog(Environment.NewLine + "Command line arguments: " + psi.Arguments + Environment.NewLine);

                                    using (Process p = new Process())
                                    {
                                        TimeSpan duration = new TimeSpan();

                                        DataReceivedEventHandler outputDataReceived = new DataReceivedEventHandler((s, e) =>
                                        {
                                            if (!string.IsNullOrWhiteSpace(e.Data))
                                            {
                                                string dataTrimmed = e.Data.Trim();

                                                if (dataTrimmed.StartsWith("Duration"))
                                                {
                                                    string durationStr = dataTrimmed.Substring(dataTrimmed.IndexOf(":") + 1).Trim();
                                                    durationStr = durationStr.Substring(0, durationStr.IndexOf(",")).Trim();
                                                    duration = TimeSpan.Parse(durationStr);
                                                }

                                                if (dataTrimmed.StartsWith("frame"))
                                                {
                                                    string timeStr = dataTrimmed.Substring(dataTrimmed.IndexOf("time") + 4).Trim();
                                                    timeStr = timeStr.Substring(timeStr.IndexOf("=") + 1).Trim();
                                                    timeStr = timeStr.Substring(0, timeStr.IndexOf(" ")).Trim();
                                                    TimeSpan current = TimeSpan.Parse(timeStr);

                                                    lock (percentageLock)
                                                    {
                                                        download.Progress = (int)(current.TotalMilliseconds * 100 / duration.TotalMilliseconds);
                                                    }
                                                }

                                                download.AppendLog(Environment.NewLine + e.Data);
                                            }
                                        });

                                        p.OutputDataReceived += outputDataReceived;
                                        p.ErrorDataReceived += outputDataReceived;
                                        p.StartInfo = psi;
                                        p.Start();
                                        p.BeginErrorReadLine();
                                        p.BeginOutputReadLine();
                                        p.WaitForExit();
                                    }

                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Encoding complete!");
                                }
                            }, cancellationTokenSource.Token);

                            Task continueTask = downloadVideoTask.ContinueWith(task =>
                            {
                                download.AppendLog(Environment.NewLine + Environment.NewLine + "Starting temporary download folder cleanup!");
                                this.CleanUp(outputDir, download);

                                download.Progress = 100;

                                if (task.IsFaulted)
                                {
                                    download.DownloadStatus = DownloadStatus.Error;
                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Download task ended with an error!");

                                    if (task.Exception != null)
                                    {
                                        download.AppendLog(Environment.NewLine + Environment.NewLine + task.Exception.ToString());
                                    }
                                }
                                else if (task.IsCanceled)
                                {
                                    download.DownloadStatus = DownloadStatus.Canceled;
                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Download task was canceled!");
                                }
                                else
                                {
                                    download.DownloadStatus = DownloadStatus.Finished;
                                    download.AppendLog(Environment.NewLine + Environment.NewLine + "Download task ended successfully!");
                                }

                                DownloadTask downloadTask;

                                if (!this.downloadTasks.TryRemove(video.Id, out downloadTask))
                                {
                                    throw new ApplicationException("Could not remove download task with ID '" + video.Id + "' from download task collection!");
                                }
                            });

                            if (this.downloadTasks.TryAdd(video.Id, new DownloadTask(downloadVideoTask, continueTask, cancellationTokenSource)))
                            {
                                downloadVideoTask.Start();
                                download.DownloadStatus = DownloadStatus.Active;
                            }
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(this.changeDownloadLockObject);
                }
            }
        }

        private void CleanUp(string directory, TwitchVideoDownload download)
        {
            try
            {
                download.AppendLog(Environment.NewLine + "Deleting directory '" + directory + "'...");
                FileSystem.DeleteDirectory(directory);
                download.AppendLog(" done!");
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
                    TwitchVideoDownload download = this.Downloads.Where(d => d.Video.Id == id).FirstOrDefault();

                    if (download != null && (download.DownloadStatus == DownloadStatus.Canceled || download.DownloadStatus == DownloadStatus.Error))
                    {
                        download.ResetLog();
                        download.Progress = 0;
                        download.DownloadStatus = DownloadStatus.Queued;
                        download.Status = "Initializing";
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
                    TwitchVideoDownload download = this.Downloads.Where(d => d.Video.Id == id).FirstOrDefault();

                    if (download != null)
                    {
                        this.Downloads.Remove(download);
                    }
                }
            }
        }

        public TwitchVideo ParseVideo(dynamic videoJson)
        {
            string title = videoJson.title;
            string id = videoJson._id;
            string game = videoJson.game;
            int views = videoJson.views;
            TimeSpan length = new TimeSpan(0, 0, (int)videoJson.length);
            List<TwitchVideoResolution> resolutions = this.ParseResolutions(videoJson.resolutions, videoJson.fps);
            DateTime recordedDate = DateTime.Parse(videoJson.recorded_at.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);
            Uri thumbnail = new Uri(videoJson.preview.ToString());
            Uri url = new Uri(videoJson.url.ToString());

            return new TwitchVideo(title, id, game, views, length, resolutions, recordedDate, thumbnail, url);
        }

        public List<TwitchVideoResolution> ParseResolutions(dynamic resolutionsJson, dynamic fpsJson)
        {
            List<TwitchVideoResolution> resolutions = new List<TwitchVideoResolution>();

            Dictionary<string, string> fpsList = new Dictionary<string, string>();

            if (fpsJson != null)
            {
                foreach (dynamic fps in fpsJson)
                {
                    fpsList.Add(fps.Name, ((int)Math.Round((double)fps.Value, 0)).ToString());
                }
            }

            foreach (dynamic resolution in resolutionsJson)
            {
                string quality = resolution.Name.ToString();

                switch (quality)
                {
                    case "chunked":
                        resolutions.Add(new TwitchVideoResolution(VideoQuality.Source, resolution.Value.ToString(), fpsList.ContainsKey("chunked") ? fpsList["chunked"] : null));
                        break;

                    case "high":
                        resolutions.Add(new TwitchVideoResolution(VideoQuality.High, resolution.Value.ToString(), fpsList.ContainsKey("high") ? fpsList["high"] : null));
                        break;

                    case "medium":
                        resolutions.Add(new TwitchVideoResolution(VideoQuality.Medium, resolution.Value.ToString(), fpsList.ContainsKey("medium") ? fpsList["medium"] : null));
                        break;

                    case "low":
                        resolutions.Add(new TwitchVideoResolution(VideoQuality.Low, resolution.Value.ToString(), fpsList.ContainsKey("low") ? fpsList["low"] : null));
                        break;

                    case "mobile":
                        resolutions.Add(new TwitchVideoResolution(VideoQuality.Mobile, resolution.Value.ToString(), fpsList.ContainsKey("mobile") ? fpsList["mobile"] : null));
                        break;
                }
            }

            Dictionary<VideoQuality, int> orderMap = new Dictionary<VideoQuality, int>();
            orderMap.Add(VideoQuality.Source, 0);
            orderMap.Add(VideoQuality.High, 1);
            orderMap.Add(VideoQuality.Medium, 2);
            orderMap.Add(VideoQuality.Low, 3);
            orderMap.Add(VideoQuality.Mobile, 4);

            resolutions = resolutions.OrderBy(r => orderMap[r.VideoQuality]).ToList();

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
                return !this.Downloads.Where(d => d.DownloadStatus == DownloadStatus.Active || d.DownloadStatus == DownloadStatus.Queued).Any();
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

            List<string> toRemove = this.downloads.Select(d => d.Video.Id).ToList();

            foreach (string id in toRemove)
            {
                this.Remove(id);
            }
        }

        #endregion Methods

        #region Nested Classes

        private class WebChunk
        {
            public WebChunk(string filename, string url)
            {
                this.Filename = filename;
                this.Url = url;
            }

            public string Filename { get; private set; }

            public string Url { get; private set; }
        }

        private class DownloadTask
        {
            public DownloadTask(Task task, Task continueTask, CancellationTokenSource cancellationTokenSource)
            {
                this.Task = task;
                this.ContinueTask = continueTask;
                this.CancellationTokenSource = cancellationTokenSource;
            }

            public Task Task { get; private set; }

            public Task ContinueTask { get; private set; }

            public CancellationTokenSource CancellationTokenSource { get; private set; }
        }

        #endregion Nested Classes
    }
}