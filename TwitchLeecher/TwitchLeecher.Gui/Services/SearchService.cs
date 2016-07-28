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

        private IEventAggregator eventAggregator;
        private IDialogService dialogService;
        private ITwitchService twitchService;
        private INavigationService navigationService;
        private IPreferencesService preferencesService;

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
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            this.twitchService = twitchService;
            this.navigationService = navigationService;
            this.preferencesService = preferencesService;

            this.eventAggregator.GetEvent<PreferencesSavedEvent>().Subscribe(this.PreferencesSaved);
        }

        #endregion Constructors

        #region Properties

        public SearchParameters LastSearchParams
        {
            get
            {
                if (this.lastSearchParams == null)
                {
                    Preferences currentPrefs = this.preferencesService.CurrentPreferences;

                    SearchParameters defaultParams = new SearchParameters(SearchType.Channel)
                    {
                        Username = currentPrefs.SearchChannelName,
                        VideoType = currentPrefs.SearchVideoType,
                        LoadLimit = currentPrefs.SearchLoadLimit
                    };

                    this.lastSearchParams = defaultParams;
                }

                return this.lastSearchParams;
            }
        }

        #endregion Properties

        #region Methods

        public void PerformSearch(SearchParameters searchParams)
        {
            this.lastSearchParams = searchParams;

            this.navigationService.ShowLoading();

            Task searchTask = new Task(() => this.twitchService.Search(searchParams));

            searchTask.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    this.dialogService.ShowAndLogException(task.Exception);
                }

                this.navigationService.ShowSearchResults();
            }, TaskScheduler.FromCurrentSynchronizationContext());

            searchTask.Start();
        }

        private void PreferencesSaved()
        {
            try
            {
                this.lastSearchParams = null;
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        #endregion Methods
    }
}