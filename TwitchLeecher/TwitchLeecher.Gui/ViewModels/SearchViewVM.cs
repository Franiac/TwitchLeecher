using System;
using System.Collections.Generic;
using System.Windows.Input;
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

                if (!string.IsNullOrWhiteSpace(this.SearchParams.Username) && !this.twitchService.UserExists(this.SearchParams.Username))
                {
                    this.SearchParams.AddError(nameof(this.SearchParams.Username), "The specified username does not exist on Twitch!");
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