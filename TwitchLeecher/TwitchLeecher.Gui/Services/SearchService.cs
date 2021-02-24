using System;
using System.Threading.Tasks;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.Services
{
    internal class SearchService : ISearchService
    {
        #region Fields

        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogService _dialogService;
        private readonly ITwitchService _twitchService;
        private readonly INavigationService _navigationService;
        private readonly IPreferencesService _preferencesService;

        private SearchParameters lastSearchParams;

        #endregion Fields

        #region Constructors

        public SearchService(
            IEventAggregator eventAggregator,
            IDialogService dialogService,
            ITwitchService twitchService,
            INavigationService navigationService,
            IPreferencesService preferencesService)
        {
            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
            _twitchService = twitchService;
            _navigationService = navigationService;
            _preferencesService = preferencesService;

            _eventAggregator.GetEvent<PreferencesSavedEvent>().Subscribe(PreferencesSaved);
        }

        #endregion Constructors

        #region Properties

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

            Task searchTask = new Task(() => _twitchService.Search(searchParams));

            searchTask.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    _dialogService.ShowAndLogException(task.Exception);
                }

                _navigationService.ShowSearchResults();
            }, TaskScheduler.FromCurrentSynchronizationContext());

            searchTask.Start();
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

        #endregion Methods
    }
}