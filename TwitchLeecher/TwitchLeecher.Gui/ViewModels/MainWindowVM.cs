using Ninject;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Services;
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

        private ViewModelBase mainView;

        private IKernel kernel;
        private IEventAggregator eventAggregator;
        private ITwitchService twitchService;
        private IGuiService guiService;
        private IPreferencesService preferencesService;
        private IUpdateService updateService;

        private ICommand searchCommand;
        private ICommand showVideosCommand;
        private ICommand showDownloadsCommand;
        private ICommand preferencesCommand;
        private ICommand infoCommand;
        private ICommand minimizeCommand;
        private ICommand maximizeRestoreCommand;
        private ICommand closeCommand;
        private ICommand requestCloseCommand;

        private WindowState windowState;

        private SearchParameters lastSearchParams;

        private Dictionary<Type, ViewModelBase> mainViews;

        #endregion Fields

        #region Constructors

        public MainWindowVM(IKernel kernel,
            IEventAggregator eventAggregator,
            ITwitchService twitchService,
            IGuiService guiService,
            IPreferencesService preferencesService,
            IUpdateService updateService)
        {
            this.mainViews = new Dictionary<Type, ViewModelBase>();

            this.windowState = WindowState.Normal;

            AssemblyUtil au = AssemblyUtil.Get;

            this.title = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();

            this.kernel = kernel;
            this.eventAggregator = eventAggregator;
            this.twitchService = twitchService;
            this.guiService = guiService;
            this.preferencesService = preferencesService;
            this.updateService = updateService;

            this.twitchService.Downloads.CollectionChanged += TwitchServiceDownloads_CollectionChanged;

            this.eventAggregator.GetEvent<SearchBeginEvent>().Subscribe(() => this.ShowMainView<VideosLoadingViewVM>());
            this.eventAggregator.GetEvent<SearchCompleteEvent>().Subscribe(() => this.ShowMainView<VideosViewVM>());
            this.eventAggregator.GetEvent<ShowVideosEvent>().Subscribe(() => this.ShowMainView<VideosViewVM>());
            this.eventAggregator.GetEvent<ShowDownloadsEvent>().Subscribe(() => this.ShowMainView<DownloadsViewVM>());
            this.eventAggregator.GetEvent<ShowPreferencesEvent>().Subscribe(() => this.ShowMainView<PreferencesViewVM>());
            this.eventAggregator.GetEvent<ShowInfoEvent>().Subscribe(() => this.ShowMainView<InfoViewVM>());
            this.eventAggregator.GetEvent<PreferencesSavedEvent>().Subscribe(() => this.PreferencesSaved());

            this.ShowMainView<WelcomeViewVM>();
        }

        #endregion Constructors

        #region Properties

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

        public int DownloadsCount
        {
            get
            {
                return this.twitchService.Downloads.Count;
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

        public ICommand SearchCommand
        {
            get
            {
                if (this.searchCommand == null)
                {
                    this.searchCommand = new DelegateCommand(this.SearchDialog);
                }

                return this.searchCommand;
            }
        }

        public ICommand ShowVideosCommand
        {
            get
            {
                if (this.showVideosCommand == null)
                {
                    this.showVideosCommand = new DelegateCommand(() =>
                    {
                        try
                        {
                            this.eventAggregator.GetEvent<ShowVideosEvent>().Publish();
                        }
                        catch (Exception ex)
                        {
                            this.guiService.ShowAndLogException(ex);
                        }
                    });
                }

                return this.showVideosCommand;
            }
        }

        public ICommand ShowDownloadsCommand
        {
            get
            {
                if (this.showDownloadsCommand == null)
                {
                    this.showDownloadsCommand = new DelegateCommand(() =>
                    {
                        try
                        {
                            this.eventAggregator.GetEvent<ShowDownloadsEvent>().Publish();
                        }
                        catch (Exception ex)
                        {
                            this.guiService.ShowAndLogException(ex);
                        }
                    });
                }

                return this.showDownloadsCommand;
            }
        }

        public ICommand PreferencesCommand
        {
            get
            {
                if (this.preferencesCommand == null)
                {
                    this.preferencesCommand = new DelegateCommand(() =>
                    {
                        try
                        {
                            this.eventAggregator.GetEvent<ShowPreferencesEvent>().Publish();
                        }
                        catch (Exception ex)
                        {
                            this.guiService.ShowAndLogException(ex);
                        }
                    });
                }

                return this.preferencesCommand;
            }
        }

        public ICommand InfoCommand
        {
            get
            {
                if (this.infoCommand == null)
                {
                    this.infoCommand = new DelegateCommand(() =>
                    {
                        try
                        {
                            this.eventAggregator.GetEvent<ShowInfoEvent>().Publish();
                        }
                        catch (Exception ex)
                        {
                            this.guiService.ShowAndLogException(ex);
                        }
                    });
                }

                return this.infoCommand;
            }
        }

        public ICommand MinimizeCommand
        {
            get
            {
                if (this.minimizeCommand == null)
                {
                    this.minimizeCommand = new DelegateCommand<Window>(window =>
                    {
                        try
                        {
                            window.WindowState = WindowState.Minimized;
                        }
                        catch (Exception ex)
                        {
                            this.guiService.ShowAndLogException(ex);
                        }
                    });
                }

                return this.minimizeCommand;
            }
        }

        public ICommand MaximizeRestoreCommand
        {
            get
            {
                if (this.maximizeRestoreCommand == null)
                {
                    this.maximizeRestoreCommand = new DelegateCommand<Window>(window =>
                    {
                        try
                        {
                            window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
                        }
                        catch (Exception ex)
                        {
                            this.guiService.ShowAndLogException(ex);
                        }
                    });
                }

                return this.maximizeRestoreCommand;
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (this.closeCommand == null)
                {
                    this.closeCommand = new DelegateCommand<Window>(window =>
                    {
                        try
                        {
                            window.Close();
                        }
                        catch (Exception ex)
                        {
                            this.guiService.ShowAndLogException(ex);
                        }
                    });
                }

                return this.closeCommand;
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

        private void SearchDialog()
        {
            try
            {
                if (this.lastSearchParams == null)
                {
                    Preferences currentPrefs = this.preferencesService.CurrentPreferences;
                    this.lastSearchParams = new SearchParameters(currentPrefs.SearchChannelName, currentPrefs.SearchVideoType, currentPrefs.SearchLoadLimit);
                }

                this.guiService.ShowSearchDialog(lastSearchParams, this.SearchDialogCallback);
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        public void SearchDialogCallback(bool cancelled, SearchParameters searchParams)
        {
            try
            {
                if (!cancelled)
                {
                    this.Search(searchParams);
                }
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        private void Search(SearchParameters searchParams)
        {
            this.lastSearchParams = searchParams;

            this.eventAggregator.GetEvent<SearchBeginEvent>().Publish();

            Task searchTask = new Task(() => this.twitchService.Search(searchParams));

            searchTask.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    this.guiService.ShowAndLogException(task.Exception);
                }

                this.eventAggregator.GetEvent<SearchCompleteEvent>().Publish();
            }, TaskScheduler.FromCurrentSynchronizationContext());

            searchTask.Start();
        }

        private void ShowMainView<T>() where T : ViewModelBase
        {
            this.MainView?.OnBeforeHidden();

            ViewModelBase vm;

            if (!this.mainViews.TryGetValue(typeof(T), out vm))
            {
                vm = this.kernel.Get<T>();
                this.mainViews.Add(typeof(T), vm);
            }

            vm.OnBeforeShown();

            this.MainView = vm;
        }

        private void PreferencesSaved()
        {
            try
            {
                this.lastSearchParams = null;
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        public void Loaded()
        {
            try
            {
                Preferences currentPrefs = this.preferencesService.CurrentPreferences.Clone();

                if (currentPrefs.SearchOnStartup)
                {
                    currentPrefs.Validate();

                    if (!currentPrefs.HasErrors)
                    {
                        SearchParameters searchParams = new SearchParameters(currentPrefs.SearchChannelName, currentPrefs.SearchVideoType, currentPrefs.SearchLoadLimit);
                        this.Search(searchParams);
                    }
                }

                if (currentPrefs.AppCheckForUpdates)
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(500);
                    }).ContinueWith(task =>
                    {
                        UpdateInfo updateInfo = this.updateService.CheckForUpdate();

                        if (updateInfo != null)
                        {
                            this.guiService.ShowUpdateInfoWindow(updateInfo);
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        private bool CloseApplication()
        {
            try
            {
                this.twitchService.Pause();

                if (!this.twitchService.CanShutdown())
                {
                    MessageBoxResult result = this.guiService.ShowMessageBox("Do you want to abort all running downloads and exit the application?", "Exit Application", MessageBoxButton.YesNo, MessageBoxImage.Warning);

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
                this.guiService.ShowAndLogException(ex);
            }

            return true;
        }

        #endregion Methods

        #region EventHandlers

        private void TwitchServiceDownloads_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.FirePropertyChanged(nameof(this.DownloadsCount));
        }

        #endregion EventHandlers
    }
}