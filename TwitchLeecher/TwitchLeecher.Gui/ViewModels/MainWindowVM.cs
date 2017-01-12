using Ninject;
using System;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Events;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Notification;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Gui.ViewModels
{
    public class MainWindowVM : BindableBase
    {
        #region Fields

        private string title;

        private bool isAuthorized;

        private int videosCount;
        private int downloadsCount;

        private ViewModelBase mainView;

        private IKernel kernel;
        private IEventAggregator eventAggregator;
        private ITwitchService twitchService;
        private IDialogService dialogService;
        private INavigationService navigationService;
        private ISearchService searchService;
        private IPreferencesService preferencesService;
        private IRuntimeDataService runtimeDataService;
        private IUpdateService updateService;

        private ICommand showSearchCommand;
        private ICommand showDownloadsCommand;
        private ICommand showAuthorizeCommand;
        private ICommand showPreferencesCommand;
        private ICommand showInfoCommand;
        private ICommand doMinimizeCommand;
        private ICommand doMmaximizeRestoreCommand;
        private ICommand doCloseCommand;
        private ICommand requestCloseCommand;

        private WindowState windowState;

        private readonly object commandLockObject;

        #endregion Fields

        #region Constructors

        public MainWindowVM(IKernel kernel,
            IEventAggregator eventAggregator,
            ITwitchService twitchService,
            IDialogService dialogService,
            INavigationService navigationService,
            ISearchService searchService,
            IPreferencesService preferencesService,
            IRuntimeDataService runtimeDataService,
            IUpdateService updateService)
        {
            this.windowState = WindowState.Normal;

            AssemblyUtil au = AssemblyUtil.Get;

            this.title = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();

            this.kernel = kernel;
            this.eventAggregator = eventAggregator;
            this.twitchService = twitchService;
            this.dialogService = dialogService;
            this.navigationService = navigationService;
            this.searchService = searchService;
            this.preferencesService = preferencesService;
            this.runtimeDataService = runtimeDataService;
            this.updateService = updateService;

            this.commandLockObject = new object();

            this.eventAggregator.GetEvent<ShowViewEvent>().Subscribe(this.ShowView);
            this.eventAggregator.GetEvent<IsAuthorizedChangedEvent>().Subscribe(this.IsAuthorizedChanged);
            this.eventAggregator.GetEvent<VideosCountChangedEvent>().Subscribe(this.VideosCountChanged);
            this.eventAggregator.GetEvent<DownloadsCountChangedEvent>().Subscribe(this.DownloadsCountChanged);
        }

        #endregion Constructors

        #region Properties

        public bool IsAuthorized
        {
            get
            {
                return this.isAuthorized;
            }
            private set
            {
                this.SetProperty(ref this.isAuthorized, value, nameof(this.IsAuthorized));
            }
        }

        public int VideosCount
        {
            get
            {
                return this.videosCount;
            }
            private set
            {
                this.SetProperty(ref this.videosCount, value, nameof(this.VideosCount));
            }
        }

        public int DownloadsCount
        {
            get
            {
                return this.downloadsCount;
            }
            private set
            {
                this.SetProperty(ref this.downloadsCount, value, nameof(this.DownloadsCount));
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }
        }

        public bool IsMaximized
        {
            get
            {
                return this.windowState == WindowState.Maximized;
            }
        }

        public ViewModelBase MainView
        {
            get
            {
                return this.mainView;
            }
            set
            {
                this.SetProperty(ref this.mainView, value, nameof(this.MainView));
            }
        }

        public WindowState WindowState
        {
            get
            {
                return this.windowState;
            }
            set
            {
                this.SetProperty(ref this.windowState, value, nameof(this.WindowState));
                this.FirePropertyChanged(nameof(this.IsMaximized));
            }
        }

        public ICommand ShowSearchCommand
        {
            get
            {
                if (this.showSearchCommand == null)
                {
                    this.showSearchCommand = new DelegateCommand(this.ShowSearch);
                }

                return this.showSearchCommand;
            }
        }

        public ICommand ShowDownloadsCommand
        {
            get
            {
                if (this.showDownloadsCommand == null)
                {
                    this.showDownloadsCommand = new DelegateCommand(this.ShowDownloads);
                }

                return this.showDownloadsCommand;
            }
        }

        public ICommand ShowAuthorizeCommand
        {
            get
            {
                if (this.showAuthorizeCommand == null)
                {
                    this.showAuthorizeCommand = new DelegateCommand(this.ShowAuthorize);
                }

                return this.showAuthorizeCommand;
            }
        }

        public ICommand ShowPreferencesCommand
        {
            get
            {
                if (this.showPreferencesCommand == null)
                {
                    this.showPreferencesCommand = new DelegateCommand(this.ShowPreferences);
                }

                return this.showPreferencesCommand;
            }
        }

        public ICommand ShowInfoCommand
        {
            get
            {
                if (this.showInfoCommand == null)
                {
                    this.showInfoCommand = new DelegateCommand(this.ShowInfo);
                }

                return this.showInfoCommand;
            }
        }

        public ICommand DoMinimizeCommand
        {
            get
            {
                if (this.doMinimizeCommand == null)
                {
                    this.doMinimizeCommand = new DelegateCommand<Window>(this.DoMinimize);
                }

                return this.doMinimizeCommand;
            }
        }

        public ICommand DoMaximizeRestoreCommand
        {
            get
            {
                if (this.doMmaximizeRestoreCommand == null)
                {
                    this.doMmaximizeRestoreCommand = new DelegateCommand<Window>(this.DoMaximizeRestore);
                }

                return this.doMmaximizeRestoreCommand;
            }
        }

        public ICommand DoCloseCommand
        {
            get
            {
                if (this.doCloseCommand == null)
                {
                    this.doCloseCommand = new DelegateCommand<Window>(this.DoClose);
                }

                return this.doCloseCommand;
            }
        }

        public ICommand RequestCloseCommand
        {
            get
            {
                if (this.requestCloseCommand == null)
                {
                    this.requestCloseCommand = new DelegateCommand(() => { }, this.CloseApplication);
                }

                return this.requestCloseCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void ShowSearch()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    if (this.videosCount > 0)
                    {
                        this.navigationService.ShowSearchResults();
                    }
                    else
                    {
                        this.navigationService.ShowSearch();
                    }
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void ShowDownloads()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.navigationService.ShowDownloads();
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void ShowAuthorize()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    if (this.twitchService.IsAuthorized)
                    {
                        this.navigationService.ShowRevokeAuthorization();
                    }
                    else
                    {
                        this.navigationService.ShowAuthorize();
                    }
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void ShowPreferences()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.navigationService.ShowPreferences();
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void ShowInfo()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.navigationService.ShowInfo();
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void DoMinimize(Window window)
        {
            try
            {
                lock (this.commandLockObject)
                {
                    if (window == null)
                    {
                        throw new ArgumentNullException(nameof(window));
                    }

                    window.WindowState = WindowState.Minimized;
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void DoMaximizeRestore(Window window)
        {
            try
            {
                lock (this.commandLockObject)
                {
                    if (window == null)
                    {
                        throw new ArgumentNullException(nameof(window));
                    }

                    window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void DoClose(Window window)
        {
            try
            {
                lock (this.commandLockObject)
                {
                    if (window == null)
                    {
                        throw new ArgumentNullException(nameof(window));
                    }

                    window.Close();
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private void ShowView(ViewModelBase contentVM)
        {
            if (contentVM != null)
            {
                this.MainView = contentVM;
            }
        }

        private void IsAuthorizedChanged(bool isAuthorized)
        {
            this.IsAuthorized = isAuthorized;
        }

        private void VideosCountChanged(int count)
        {
            this.VideosCount = count;
        }

        private void DownloadsCountChanged(int count)
        {
            this.DownloadsCount = count;
        }

        public void Loaded()
        {
            try
            {
                Preferences currentPrefs = this.preferencesService.CurrentPreferences.Clone();

                bool updateAvailable = false;

                if (currentPrefs.AppCheckForUpdates)
                {
                    UpdateInfo updateInfo = this.updateService.CheckForUpdate();

                    if (updateInfo != null)
                    {
                        updateAvailable = true;
                        this.navigationService.ShowUpdateInfo(updateInfo);
                    }
                }

                bool searchOnStartup = false;

                if (!updateAvailable && currentPrefs.SearchOnStartup)
                {
                    currentPrefs.Validate();

                    if (!currentPrefs.HasErrors)
                    {
                        searchOnStartup = true;

                        SearchParameters searchParams = new SearchParameters(SearchType.Channel)
                        {
                            Channel = currentPrefs.SearchChannelName,
                            VideoType = currentPrefs.SearchVideoType,
                            LoadLimit = currentPrefs.SearchLoadLimit
                        };

                        this.searchService.PerformSearch(searchParams);
                    }
                }

                if (!updateAvailable && !searchOnStartup)
                {
                    this.navigationService.ShowWelcome();
                }

                this.twitchService.Authorize(this.runtimeDataService.RuntimeData.AccessToken);
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        private bool CloseApplication()
        {
            try
            {
                this.twitchService.Pause();

                if (!this.twitchService.CanShutdown())
                {
                    MessageBoxResult result = this.dialogService.ShowMessageBox("Do you want to abort all running downloads and exit the application?", "Exit Application", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                    {
                        this.twitchService.Resume();
                        return false;
                    }
                }

                this.twitchService.Shutdown();
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }

            return true;
        }

        #endregion Methods
    }
}