using System;
using System.Collections.Generic;
using System.Windows.Input;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Helpers;

namespace TwitchLeecher.Gui.ViewModels
{
    public class SearchViewVM : ViewModelBase
    {
        #region Fields

        private SearchParameters _searchParams;

        private ICommand _clearUrlsCommand;
        private ICommand _clearIdsCommand;
        private ICommand _searchCommand;
        private ICommand _cancelCommand;

        private readonly IApiService _apiService;
        private readonly ISearchService _searchService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IPreferencesService _preferencesService;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructors

        public SearchViewVM(
            IApiService apiService,
            ISearchService searchService,
            IDialogService dialogService,
            INavigationService navigationService,
            IPreferencesService preferencesService)
        {
            _apiService = apiService;
            _searchService = searchService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _preferencesService = preferencesService;

            _commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public SearchParameters SearchParams
        {
            get
            {
                if (_searchParams == null)
                {
                    _searchParams = _searchService.LastSearchParams.Clone();
                }

                return _searchParams;
            }
            set
            {
                SetProperty(ref _searchParams, value, nameof(SearchParams));
            }
        }

        public RangeObservableCollection<string> FavChannels
        {
            get
            {
                return _preferencesService.CurrentPreferences.SearchFavouriteChannels;
            }
        }

        public ICommand ClearUrlsCommand
        {
            get
            {
                if (_clearUrlsCommand == null)
                {
                    _clearUrlsCommand = new DelegateCommand(ClearUrls);
                }

                return _clearUrlsCommand;
            }
        }

        public ICommand ClearIdsCommand
        {
            get
            {
                if (_clearIdsCommand == null)
                {
                    _clearIdsCommand = new DelegateCommand(ClearIds);
                }

                return _clearIdsCommand;
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                {
                    _searchCommand = new DelegateCommand(Search);
                }

                return _searchCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new DelegateCommand(Cancel);
                }

                return _cancelCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void ClearUrls()
        {
            try
            {
                lock (_commandLockObject)
                {
                    SearchParams.Urls = null;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void ClearIds()
        {
            try
            {
                lock (_commandLockObject)
                {
                    SearchParams.Ids = null;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void Search()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _dialogService.SetBusy();
                    Validate();

                    if (!HasErrors)
                    {
                        _searchService.PerformSearch(SearchParams);
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void Cancel()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _navigationService.NavigateBack();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(SearchParams);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                SearchParams.Validate();

                if (SearchParams.SearchType == SearchType.Channel &&
                    !string.IsNullOrWhiteSpace(SearchParams.Channel) &&
                    !_apiService.ChannelExists(SearchParams.Channel))
                {
                    SearchParams.AddError(nameof(SearchParams.Channel), "The specified channel does not exist on Twitch!");
                }

                if (SearchParams.HasErrors)
                {
                    AddError(currentProperty, "Invalid Search Parameters!");
                }
            }
        }

        protected override List<MenuCommand> BuildMenu()
        {
            List<MenuCommand> menuCommands = base.BuildMenu();

            if (menuCommands == null)
            {
                menuCommands = new List<MenuCommand>();
            }

            menuCommands.Add(new MenuCommand(SearchCommand, "Search", "Solid_MagnifyingGlass"));
            menuCommands.Add(new MenuCommand(CancelCommand, "Cancel", "Solid_XMark"));

            return menuCommands;
        }

        #endregion Methods
    }
}