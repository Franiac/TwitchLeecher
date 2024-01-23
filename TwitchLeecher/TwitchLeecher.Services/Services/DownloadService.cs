using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
    internal class DownloadService : BindableBase, IDownloadService, IDisposable
    {
        #region Constants

        private const int TIMER_INTERVALL = 2;
        private const int DOWNLOAD_RETRIES = 3;
        private const int DOWNLOAD_RETRY_TIME = 5;

        #endregion Constants

        #region Fields

        private bool disposedValue = false;

        private readonly IApiService _apiService;
        private readonly IPreferencesService _preferencesService;
        private readonly IProcessingService _processingService;
        private readonly IEventAggregator _eventAggregator;

        private readonly Timer _downloadTimer;

        private ObservableCollection<TwitchVideoDownload> _downloads;

        private ConcurrentDictionary<string, DownloadTask> _downloadTasks;

        private readonly object _changeDownloadLockObject;

        private volatile bool _paused;

        #endregion Fields

        #region Constructors

        public DownloadService(
            IApiService apiService,
            IPreferencesService preferencesService,
            IProcessingService processingService,
            IEventAggregator eventAggregator)
        {
            _apiService = apiService;
            _preferencesService = preferencesService;
            _processingService = processingService;
            _eventAggregator = eventAggregator;

            _downloads = new ObservableCollection<TwitchVideoDownload>();
            _downloads.CollectionChanged += Downloads_CollectionChanged;

            _downloadTasks = new ConcurrentDictionary<string, DownloadTask>();

            _changeDownloadLockObject = new object();

            _downloadTimer = new Timer(DownloadTimerCallback, null, 0, TIMER_INTERVALL);

            _eventAggregator.GetEvent<RemoveDownloadEvent>().Subscribe(Remove, ThreadOption.UIThread);
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<TwitchVideoDownload> Downloads
        {
            get { return _downloads; }
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
                        TwitchVideoDownload download = _downloads.Where(d => d.DownloadState == DownloadState.Queued)
                            .FirstOrDefault();

                        if (download == null)
                        {
                            return;
                        }

                        DownloadParameters downloadParams = download.DownloadParams;

                        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                        CancellationToken cancellationToken = cancellationTokenSource.Token;

                        string downloadId = download.Id;
                        string vodId = downloadParams.Video.Id;
                        string tempDir = Path.Combine(_preferencesService.CurrentPreferences.DownloadTempFolder,
                            downloadId);
                        string ffmpegFile = _processingService.FFMPEGExe;
                        string concatFile = Path.Combine(tempDir,
                            Path.GetFileNameWithoutExtension(downloadParams.FullPath) + ".ts");
                        string outputFile = downloadParams.FullPath;

                        bool disableConversion = downloadParams.DisableConversion;
                        bool cropStart = downloadParams.CropStart;
                        bool cropEnd = downloadParams.CropEnd;

                        TimeSpan cropStartTime = downloadParams.CropStartTime;
                        TimeSpan cropEndTime = downloadParams.CropEndTime;

                        TwitchVideoQuality quality = downloadParams.SelectedQuality;

                        Action<DownloadState> setDownloadState = download.SetDownloadState;
                        Action<string> log = download.AppendLog;
                        Action<string> setStatus = download.SetStatus;
                        Action<double> setProgress = download.SetProgress;
                        Action<bool> setIsIndeterminate = download.SetIsIndeterminate;

                        Task<IEnumerable<string>> downloadVideoTask = new Task<IEnumerable<string>>(() =>
                        {
                            setStatus("Initializing");

                            log("Download task has been started!");

                            WriteDownloadInfo(log, downloadParams, ffmpegFile, tempDir);

                            cancellationToken.ThrowIfCancellationRequested();

                            log(Environment.NewLine + Environment.NewLine + "Retrieving VOD access information...");
                            TwitchVideoAuthInfo vodAuthInfo = _apiService.GetVodAuthInfo(vodId);
                            log(" done!");

                            cancellationToken.ThrowIfCancellationRequested();

                            WriteVodAuthInfo(log, vodAuthInfo);

                            cancellationToken.ThrowIfCancellationRequested();

                            CheckTempDirectory(log, tempDir);

                            cancellationToken.ThrowIfCancellationRequested();

                            log(Environment.NewLine + Environment.NewLine +
                                "Retrieving playlist information for all VOD qualities...");
                            Dictionary<TwitchVideoQuality, string> playlistInfo =
                                _apiService.GetPlaylistInfo(vodId, vodAuthInfo);
                            log(" done!");

                            cancellationToken.ThrowIfCancellationRequested();

                            WritePlaylistInfo(log, playlistInfo);

                            cancellationToken.ThrowIfCancellationRequested();

                            TwitchPlaylist vodPlaylist = GetVodPlaylist(log, tempDir, playlistInfo, quality);

                            cancellationToken.ThrowIfCancellationRequested();

                            CropInfo cropInfo = CropVodPlaylist(vodPlaylist, cropStart, cropEnd, cropStartTime,
                                cropEndTime);

                            cancellationToken.ThrowIfCancellationRequested();

                            var downloadWarnings = DownloadParts(log, setStatus, setProgress, vodPlaylist,
                                cancellationToken);

                            cancellationToken.ThrowIfCancellationRequested();

                            CheckOutputDirectory(log, Path.GetDirectoryName(outputFile));

                            cancellationToken.ThrowIfCancellationRequested();

                            _processingService.ConcatParts(log, setStatus, setProgress, vodPlaylist,
                                disableConversion ? outputFile : concatFile);

                            if (!disableConversion)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                _processingService.ConvertVideo(log, setStatus, setProgress, setIsIndeterminate,
                                    concatFile, outputFile, cropInfo);
                            }

                            return downloadWarnings;
                        }, cancellationToken);

                        Task continueTask = downloadVideoTask.ContinueWith((Task<IEnumerable<string>> task) =>
                        {
                            log(Environment.NewLine + Environment.NewLine +
                                "Starting temporary download folder cleanup!");
                            CleanUp(tempDir, log);

                            setProgress(100);
                            setIsIndeterminate(false);

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
                                var warnings = task.Result.ToArray();
                                if (warnings.Any())
                                {
                                    var warningsString = string.Join("\n", warnings.Distinct());
                                    setDownloadState(DownloadState.CompletedWithWarning);
                                    log(Environment.NewLine + Environment.NewLine +
                                        $"Download completed with warnings:\n{warningsString}");
                                }
                                else
                                {
                                    setDownloadState(DownloadState.Done);
                                    log(Environment.NewLine + Environment.NewLine +
                                        "Download task ended successfully!");
                                }
                            }

                            if (!_downloadTasks.TryRemove(downloadId, out DownloadTask downloadTask))
                            {
                                throw new ApplicationException("Could not remove download task with ID '" + downloadId +
                                                               "' from download task collection!");
                            }

                            if (success && _preferencesService.CurrentPreferences.DownloadRemoveCompleted)
                            {
                                _eventAggregator.GetEvent<RemoveDownloadEvent>().Publish(downloadId);
                            }
                        });

                        if (_downloadTasks.TryAdd(downloadId,
                                new DownloadTask(downloadVideoTask, continueTask, cancellationTokenSource)))
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

        private void WriteDownloadInfo(Action<string> log, DownloadParameters downloadParams, string ffmpegFile,
            string tempDir)
        {
            log(Environment.NewLine + Environment.NewLine + "TWITCH LEECHER INFO");
            log(Environment.NewLine +
                "--------------------------------------------------------------------------------------------");
            log(Environment.NewLine + "Version: " + AssemblyUtil.Get.GetAssemblyVersion().Trim());

            log(Environment.NewLine + Environment.NewLine + "VOD INFO");
            log(Environment.NewLine +
                "--------------------------------------------------------------------------------------------");
            log(Environment.NewLine + "VOD ID: " + downloadParams.Video.Id);
            log(Environment.NewLine + "Selected Quality: " + downloadParams.SelectedQuality.DisplayString);
            log(Environment.NewLine + "Download Url: " + downloadParams.Video.Url);
            log(Environment.NewLine + "Crop Start: " + (downloadParams.CropStart
                ? "Yes (" + downloadParams.CropStartTime.ToDaylessString() + ")"
                : "No"));
            log(Environment.NewLine + "Crop End: " + (downloadParams.CropEnd
                ? "Yes (" + downloadParams.CropEndTime.ToDaylessString() + ")"
                : "No"));

            log(Environment.NewLine + Environment.NewLine + "OUTPUT INFO");
            log(Environment.NewLine +
                "--------------------------------------------------------------------------------------------");
            log(Environment.NewLine + "Disable Conversion: " + (downloadParams.DisableConversion ? "Yes" : "No"));
            log(Environment.NewLine + "Output File: " + downloadParams.FullPath);
            log(Environment.NewLine + "FFMPEG Path: " + ffmpegFile);
            log(Environment.NewLine + "Temporary Download Folder: " + tempDir);
        }

        private void WriteVodAuthInfo(Action<string> log, TwitchVideoAuthInfo vodAuthInfo)
        {
            log(Environment.NewLine + Environment.NewLine + "ACCESS INFO");
            log(Environment.NewLine +
                "--------------------------------------------------------------------------------------------");
            log(Environment.NewLine + "Token: " + vodAuthInfo.Token);
            log(Environment.NewLine + "Signature: " + vodAuthInfo.Signature);
            log(Environment.NewLine + "Sub-Only: " + (vodAuthInfo.SubOnly ? "Yes" : "No"));
            log(Environment.NewLine + "Privileged: " + (vodAuthInfo.Privileged ? "Yes" : "No"));
        }

        private void WritePlaylistInfo(Action<string> log, Dictionary<TwitchVideoQuality, string> playlistInfo)
        {
            log(Environment.NewLine + Environment.NewLine + "PLAYLIST INFO");
            log(Environment.NewLine +
                "--------------------------------------------------------------------------------------------");

            foreach (var entry in playlistInfo)
            {
                log(Environment.NewLine + "Playlist for quality '" + entry.Key.Name + "' is '" + entry.Value + "'");
            }
        }

        private void CheckTempDirectory(Action<string> log, string tempDir)
        {
            if (!Directory.Exists(tempDir))
            {
                log(Environment.NewLine + Environment.NewLine + "Creating temporary download directory '" + tempDir +
                    "'...");
                FileSystem.CreateDirectory(tempDir);
                log(" done!");
            }

            if (Directory.EnumerateFileSystemEntries(tempDir).Any())
            {
                throw new ApplicationException("Temporary download directory '" + tempDir + "' is not empty!");
            }
        }

        private void CheckOutputDirectory(Action<string> log, string outputDir)
        {
            if (!Directory.Exists(outputDir))
            {
                log(Environment.NewLine + Environment.NewLine + "Creating output directory '" + outputDir + "'...");
                FileSystem.CreateDirectory(outputDir);
                log(" done!");
            }
        }

        private TwitchPlaylist GetVodPlaylist(Action<string> log, string tempDir,
            Dictionary<TwitchVideoQuality, string> playlistInfo, TwitchVideoQuality selectedQuality)
        {
            TwitchVideoQuality quality = playlistInfo.Keys.First(q => q.Equals(selectedQuality));

            string playlistUrl = playlistInfo[quality];

            log(Environment.NewLine + Environment.NewLine + "Playlist url for selected quality '" +
                quality.DisplayString + "' is '" + playlistUrl + "'");

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
                TwitchPlaylist vodPlaylist = TwitchPlaylist.Parse(tempDir, playlistStr, urlPrefix);
                log(" done!");

                log(Environment.NewLine + "Number of video chunks: " + vodPlaylist.Count());

                return vodPlaylist;
            }
        }

        private CropInfo CropVodPlaylist(TwitchPlaylist vodPlaylist, bool cropStart, bool cropEnd,
            TimeSpan cropStartTime, TimeSpan cropEndTime)
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

            List<TwitchPlaylistPart> deleteStart = new List<TwitchPlaylistPart>();
            List<TwitchPlaylistPart> deleteEnd = new List<TwitchPlaylistPart>();

            if (cropStart)
            {
                double lengthSum = 0;

                foreach (TwitchPlaylistPart part in vodPlaylist)
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

                foreach (TwitchPlaylistPart part in vodPlaylist)
                {
                    if (lengthSum >= end)
                    {
                        deleteEnd.Add(part);
                    }

                    lengthSum += part.Length;
                }
            }

            deleteStart.ForEach(part => { vodPlaylist.Remove(part); });

            deleteEnd.ForEach(part => { vodPlaylist.Remove(part); });

            return new CropInfo(cropStart, cropEnd, cropStart ? start : 0, length);
        }

        private IEnumerable<string> DownloadParts(Action<string> log, Action<string> setStatus,
            Action<double> setProgress,
            TwitchPlaylist vodPlaylist,
            CancellationToken cancellationToken)
        {
            var warnings = new List<string>();
            int partsCount = vodPlaylist.Count;
            int maxConnectionCount = ServicePointManager.DefaultConnectionLimit;

            log(Environment.NewLine + Environment.NewLine + "Starting parallel video chunk download");
            log(Environment.NewLine + "Number of video chunks to download: " + partsCount);
            log(Environment.NewLine + "Maximum connection count: " + maxConnectionCount);

            setStatus("Downloading");

            log(Environment.NewLine + Environment.NewLine + "Parallel video chunk download is running...");

            long completedPartDownloads = 0;

            Parallel.ForEach(vodPlaylist,
                new ParallelOptions() { MaxDegreeOfParallelism = maxConnectionCount - 1 },
                (part, loopState) =>
                {
                    int retryCounter = 0;

                    bool success = false;

                    do
                    {
                        try
                        {
                            using (var downloadClient = new HttpClient())
                            {
                                byte[] bytes = downloadClient.GetByteArrayAsync(part.RemoteFile, CancellationToken.None)
                                    .GetAwaiter().GetResult();

                                Interlocked.Increment(ref completedPartDownloads);

                                FileSystem.DeleteFile(part.LocalFile);

                                File.WriteAllBytes(part.LocalFile, bytes);

                                long completed = Interlocked.Read(ref completedPartDownloads);

                                setProgress((double)completed / partsCount * 100);

                                success = true;
                            }
                        }
                        catch (WebException ex)
                        {
                            if (ex.Status == WebExceptionStatus.ProtocolError)
                            {
                                warnings.Add("Video contains missing frames");
                                log($"{Environment.NewLine}File '{part.RemoteFile}' is not available, skipping");
                                Interlocked.Increment(ref completedPartDownloads);

                                long completed = Interlocked.Read(ref completedPartDownloads);

                                setProgress((double)completed / partsCount * 100);

                                success = true;
                                continue;
                            }

                            if (retryCounter < DOWNLOAD_RETRIES)
                            {
                                retryCounter++;
                                log(Environment.NewLine + Environment.NewLine + "Downloading file '" + part.RemoteFile +
                                    "' failed! Trying again in " + DOWNLOAD_RETRY_TIME + "s");
                                log(Environment.NewLine + ex);
                                Thread.Sleep(DOWNLOAD_RETRY_TIME * 1000);
                            }
                        }
                    } while (!success);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        loopState.Stop();
                    }
                });

            setProgress(100);

            log(Environment.NewLine + Environment.NewLine + "Download of all video chunks complete!");
            return warnings;
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

                    if (download != null && (download.DownloadState == DownloadState.Canceled ||
                                             download.DownloadState == DownloadState.Error))
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
                return !_downloads.Where(d =>
                    d.DownloadState == DownloadState.Downloading || d.DownloadState == DownloadState.Queued).Any();
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
            IEnumerable<TwitchVideoDownload> downloads = _downloads.Where(d =>
                d.DownloadState == DownloadState.Downloading || d.DownloadState == DownloadState.Queued);

            foreach (TwitchVideoDownload download in downloads)
            {
                if (download.DownloadParams.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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

        private void Downloads_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireDownloadsCountChanged();
        }

        #endregion EventHandlers
    }
}