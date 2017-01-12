using System;
using System.Collections.Generic;
using System.Windows.Input;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class SearchViewVM : ViewModelBase
    {
        #region Fields

        private SearchParameters searchParams;

        private ICommand clearUrlsCommand;
        private ICommand clearIdsCommand;
        private ICommand searchCommand;
        private ICommand cancelCommand;

        private ITwitchService twitchService;
        private ISearchService searchService;
        private IDialogService dialogService;
        private INavigationService navigationService;

        private readonly object commandLockObject;

        #endregion Fields

        #region Constructors

        public SearchViewVM(
            ITwitchService twitchService,
            ISearchService searchService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            this.twitchService = twitchService;
            this.searchService = searchService;
            this.dialogService = dialogService;
            this.navigationService = navigationService;

            this.commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public SearchParameters SearchParams
        {
            get
            {
                if (this.searchParams == null)
                {
                    this.searchParams = this.searchService.LastSearchParams.Clone();
                }

                return this.searchParams;
            }
            set
            {
                this.SetProperty(ref this.searchParams, value, nameof(this.SearchParams));
            }
        }

        public ICommand ClearUrlsCommand
        {
            get
            {
                if (this.clearUrlsCommand == null)
                {
                    this.clearUrlsCommand = new DelegateCommand(this.ClearUrls);
                }

                return this.clearUrlsCommand;
            }
        }

        public ICommand ClearIdsCommand
        {
            get
            {
                if (this.clearIdsCommand == null)
                {
                    this.clearIdsCommand = new DelegateCommand(this.ClearIds);
                }

                return this.clearIdsCommand;
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                if (this.searchCommand == null)
                {
                    this.searchCommand = new DelegateCommand(this.Search);
                }

                return this.searchCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                {
                    this.cancelCommand = new DelegateCommand(this.Cancel);
                }

                return this.cancelCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void ClearUrls()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.SearchParams.Urls = null;
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void ClearIds()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.SearchParams.Ids = null;
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void Search()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.dialogService.SetBusy();
                    this.Validate();

                    if (!this.HasErrors)
                    {
                        this.searchService.PerformSearch(this.SearchParams);
                    }
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void Cancel()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.navigationService.NavigateBack();
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(this.SearchParams);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                this.SearchParams.Validate();

                if (this.SearchParams.SearchType == SearchType.Channel &&
                    !string.IsNullOrWhiteSpace(this.SearchParams.Channel) &&
                    !this.twitchService.ChannelExists(this.SearchParams.Channel))
                {
                    this.SearchParams.AddError(nameof(this.SearchParams.Channel), "The specified channel does not exist on Twitch!");
                }

                if (this.SearchParams.HasErrors)
                {
                    this.AddError(currentProperty, "Invalid Search Parameters!");
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

            menuCommands.Add(new MenuCommand(this.SearchCommand, "Search", "Search"));
            menuCommands.Add(new MenuCommand(this.CancelCommand, "Cancel", "Times"));

            return menuCommands;
        }

        #endregion Methods
    }
}