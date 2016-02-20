using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Regions;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Gui.Views;

namespace TwitchLeecher.Gui.Controllers
{
    public class MainController
    {
        #region Fields

        private IUnityContainer container;
        private IRegionManager regionManager;
        private IEventAggregator eventAggregator;

        #endregion Fields

        #region Constructor

        public MainController(IUnityContainer container, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            this.container = container;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<ShowVideosEvent>().Subscribe(this.ShowVideosView);
            this.eventAggregator.GetEvent<ShowDownloadsEvent>().Subscribe(this.ShowDownloadsView);
            this.eventAggregator.GetEvent<SearchBeginEvent>().Subscribe(this.ShowVideosLoadingView);
            this.eventAggregator.GetEvent<SearchCompleteEvent>().Subscribe(this.ShowVideosView);
        }

        #endregion Constructor

        #region Methods

        private void ShowVideosView()
        {
            IRegion mainRegion = this.regionManager.Regions[RegionNames.MainRegion];

            if (mainRegion == null)
            {
                return;
            }

            VideosView videosView = mainRegion.GetView<VideosView>() as VideosView;

            if (videosView == null)
            {
                videosView = this.container.Resolve<VideosView>();
                mainRegion.Add(videosView);
            }

            mainRegion.Activate(videosView);
        }

        private void ShowDownloadsView()
        {
            IRegion mainRegion = this.regionManager.Regions[RegionNames.MainRegion];

            if (mainRegion == null)
            {
                return;
            }

            DownloadsView downloadsView = mainRegion.GetView<DownloadsView>() as DownloadsView;

            if (downloadsView == null)
            {
                downloadsView = this.container.Resolve<DownloadsView>();
                mainRegion.Add(downloadsView);
            }

            mainRegion.Activate(downloadsView);
        }

        private void ShowVideosLoadingView()
        {
            IRegion mainRegion = this.regionManager.Regions[RegionNames.MainRegion];

            if (mainRegion == null)
            {
                return;
            }

            VideosLoadingView videosLoadingView = mainRegion.GetView<VideosLoadingView>() as VideosLoadingView;

            if (videosLoadingView == null)
            {
                videosLoadingView = this.container.Resolve<VideosLoadingView>();
                mainRegion.Add(videosLoadingView);
            }

            mainRegion.Activate(videosLoadingView);
        }

        #endregion Methods
    }
}