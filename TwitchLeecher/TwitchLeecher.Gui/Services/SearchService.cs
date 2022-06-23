using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Events;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Gui.Services
{
    internal class SearchService : BindableBase, ISearchService
    {
        #region Fields

        private readonly IEventAggregator _eventAggregator;
        private readonly IApiService _apiService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IPreferencesService _preferencesService;
        private readonly IFilenameService _filenameService;

        private ObservableCollection<TwitchVideo> _videos;

        private SearchParameters lastSearchParams;

        #endregion Fields

        #region Constructors

        public SearchService(
            IEventAggregator eventAggregator,
            IApiService apiService,
            IDialogService dialogService,
            INavigationService navigationService,
            IPreferencesService preferencesService,
            IFilenameService filenameService)
        {
            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
            _apiService = apiService;
            _navigationService = navigationService;
            _preferencesService = preferencesService;
            _filenameService = filenameService;

            _eventAggregator.GetEvent<PreferencesSavedEvent>().Subscribe(PreferencesSaved);

            _videos = new ObservableCollection<TwitchVideo>();
            _videos.CollectionChanged += Videos_CollectionChanged;
        }

        #endregion Constructors

        #region Properties

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

        public SearchParameters LastSearchParams
        {
            get
            {
                if (lastSearchParams == null)
                {
                    Preferences currentPrefs = _preferencesService.CurrentPreferences;

                    SearchParameters defaultParams = new SearchParameters(SearchType.Channel)
                    {
                        Channel = currentPrefs.SearchChannelName,
                        VideoType = currentPrefs.SearchVideoType,
                        LoadLimitType = currentPrefs.SearchLoadLimitType,
                        LoadFrom = DateTime.Now.Date.AddDays(-currentPrefs.SearchLoadLastDays),
                        LoadFromDefault = DateTime.Now.Date.AddDays(-currentPrefs.SearchLoadLastDays),
                        LoadTo = DateTime.Now.Date,
                        LoadToDefault = DateTime.Now.Date,
                        LoadLastVods = currentPrefs.SearchLoadLastVods
                    };

                    lastSearchParams = defaultParams;
                }

                return lastSearchParams;
            }
        }

        #endregion Properties

        #region Methods

        public void PerformSearch(SearchParameters searchParams)
        {
            lastSearchParams = searchParams;

            _navigationService.ShowLoading();

            Task.Run(() => _apiService.Search(searchParams)).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    _navigationService.ShowSearch();
                    _dialogService.ShowAndLogException(task.Exception);
                }

                Videos = task.Result;
                // halts spinner
                if (Videos != null && Videos.Count > 0)
                {
                    Preferences currentPrefs = _preferencesService.CurrentPreferences.Clone();

                    foreach (var video in Videos)
                    {
                        TwitchVideoAuthInfo vodAuthInfo = _apiService.GetVodAuthInfo(video.Id);
                        Dictionary<TwitchVideoQuality, string> playlistInfo = _apiService.GetPlaylistInfo(video.Id, vodAuthInfo);
                        List<TwitchVideoQuality> qualities = playlistInfo.Keys.OrderBy(q => q).ToList();

                        string folder = currentPrefs.DownloadSubfoldersForFav && _preferencesService.IsChannelInFavourites(video.Channel)
                            ? Path.Combine(currentPrefs.DownloadFolder, video.Channel)
                            : currentPrefs.DownloadFolder;
                        TwitchVideoQuality selectedQuality = GetSelectedQuality(qualities, currentPrefs.DownloadDefaultQuality);

                        string filename = _filenameService.SubstituteWildcards(currentPrefs.DownloadFileName, video, selectedQuality);
                        filename = _filenameService.EnsureExtension(filename, currentPrefs.DownloadDisableConversion);
                        video.IsDownloaded = File.Exists(Path.Combine(folder, filename));
                    }
                }

                _navigationService.ShowSearchResults();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private TwitchVideoQuality GetSelectedQuality(List<TwitchVideoQuality> qualities, DefaultQuality defaultQuality)
        {
            TwitchVideoQuality sourceQuality = qualities.Find(q => q.IsSource);

            if (sourceQuality == null)
            {
                sourceQuality = qualities[0];
            }

            if (defaultQuality.IsSource)
            {
                return sourceQuality;
            }

            if (defaultQuality.IsAudioOnly)
            {
                TwitchVideoQuality audioOnlyQuality = qualities.FirstOrDefault(q => q.IsAudioOnly);

                if (audioOnlyQuality != null)
                {
                    return audioOnlyQuality;
                }
                else
                {
                    return sourceQuality;
                }
            }

            IEnumerable<TwitchVideoQuality> visualQualities = qualities.Where(q => !q.IsAudioOnly);

            int defaultRes = defaultQuality.VerticalResolution;

            TwitchVideoQuality selectedQuality = null;

            foreach (TwitchVideoQuality quality in visualQualities)
            {
                if (quality.VerticalResolution <= defaultRes && (selectedQuality == null || selectedQuality.VerticalResolution < quality.VerticalResolution))
                {
                    selectedQuality = quality;
                }
            }

            if (selectedQuality != null)
            {
                return selectedQuality;
            }

            foreach (TwitchVideoQuality quality in visualQualities)
            {
                if (quality.VerticalResolution >= defaultRes && (selectedQuality == null || selectedQuality.VerticalResolution > quality.VerticalResolution))
                {
                    selectedQuality = quality;
                }
            }

            if (selectedQuality != null)
            {
                return selectedQuality;
            }

            return sourceQuality;
        }

        private void PreferencesSaved()
        {
            try
            {
                lastSearchParams = null;
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void FireVideosCountChanged()
        {
            _eventAggregator.GetEvent<VideosCountChangedEvent>().Publish(_videos != null ? _videos.Count : 0);
        }

        #endregion Methods

        #region EventHandlers

        private void Videos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FireVideosCountChanged();
        }

        #endregion EventHandlers
    }
}