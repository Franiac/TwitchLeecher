using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Threading.Tasks;
using System.Windows.Input;
using TwitchLeecher.Common;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Services;
using TwitchLeecher.Services.Interfaces;

namespace TwitchLeecher.Gui.ViewModels
{
    public class TitleBarVM : BindableBase
    {
        #region Fields

        private IEventAggregator eventAggregator;
        private ITwitchService twitchService;
        private IGuiService guiService;

        private string title;
        private bool isMaximized;

        private ICommand searchCommand;
        private ICommand showVideosCommand;
        private ICommand showDownloadsCommand;
        private ICommand minimizeCommand;
        private ICommand maximizeRestoreCommand;
        private ICommand closeCommand;

        private SearchParameters lastSearchParams;

        #endregion Fields

        #region Constructors

        public TitleBarVM(IEventAggregator eventAggregator, ITwitchService twitchService, IGuiService guiService)
        {
            AssemblyUtil au = AssemblyUtil.Get;

            this.title = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();

            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<AppMaximizedChangedEvent>().Subscribe(this.OnIsMaximizedChanged);

            this.twitchService = twitchService;
            this.guiService = guiService;
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
                return this.isMaximized;
            }
            set
            {
                this.SetProperty(ref this.isMaximized, value);
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

        public ICommand ShowVideosCommand
        {
            get
            {
                if (this.showVideosCommand == null)
                {
                    this.showVideosCommand = new DelegateCommand(() =>
                    {
                        this.eventAggregator.GetEvent<ShowVideosEvent>().Publish();
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
                        this.eventAggregator.GetEvent<ShowDownloadsEvent>().Publish();
                    });
                }

                return this.showDownloadsCommand;
            }
        }

        public ICommand MinimizeCommand
        {
            get
            {
                if (this.minimizeCommand == null)
                {
                    this.minimizeCommand = new DelegateCommand(() =>
                    {
                        this.eventAggregator.GetEvent<AppMinimizeEvent>().Publish();
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
                    this.maximizeRestoreCommand = new DelegateCommand(() =>
                    {
                        this.eventAggregator.GetEvent<AppMaximizeRestoreEvent>().Publish();
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
                    this.closeCommand = new DelegateCommand(() =>
                    {
                        this.eventAggregator.GetEvent<AppExitEvent>().Publish();
                    });
                }

                return this.closeCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Search()
        {
            if (this.lastSearchParams == null)
            {
                this.lastSearchParams = new SearchParameters("FakeSmileRevolution", VideoType.Broadcast, 10);
            }

            this.guiService.ShowSearchDialog(lastSearchParams, this.SearchCallback);
        }

        public void SearchCallback(bool cancelled, SearchParameters searchParams)
        {
            if (!cancelled)
            {
                this.lastSearchParams = searchParams;

                this.eventAggregator.GetEvent<SearchBeginEvent>().Publish();

                Task searchTask = new Task(() => this.twitchService.Search(searchParams));

                searchTask.ContinueWith(task => this.eventAggregator.GetEvent<SearchCompleteEvent>().Publish(), TaskScheduler.FromCurrentSynchronizationContext());

                searchTask.Start();
            }
        }

        #endregion Methods

        #region EventHandler

        private void OnIsMaximizedChanged(bool isMaximized)
        {
            this.IsMaximized = isMaximized;
        }

        #endregion EventHandler
    }
}