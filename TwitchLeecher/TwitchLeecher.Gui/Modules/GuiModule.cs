using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using TwitchLeecher.Gui.Controllers;
using TwitchLeecher.Gui.Services;
using TwitchLeecher.Gui.Views;

namespace TwitchLeecher.Gui.Modules
{
    public class GuiModule : IModule
    {
        #region Fields

        private IUnityContainer container;
        private IRegionManager regionManager;

        private MainController mainRegionController;

        #endregion Fields

        #region Constructors

        public GuiModule(IUnityContainer container, IRegionManager regionManager)
        {
            this.container = container;
            this.regionManager = regionManager;
        }

        #endregion Constructors

        #region Methods

        public void Initialize()
        {
            this.container.RegisterType<IGuiService, GuiService>(new ContainerControlledLifetimeManager());

            this.mainRegionController = this.container.Resolve<MainController>();

            this.regionManager.RegisterViewWithRegion(RegionNames.TitleBarRegion, () => this.container.Resolve<TitleBarView>());
            this.regionManager.RegisterViewWithRegion(RegionNames.MainRegion, () => this.container.Resolve<WelcomeView>());
            this.regionManager.RegisterViewWithRegion(RegionNames.MainRegion, () => this.container.Resolve<VideosView>());
            this.regionManager.RegisterViewWithRegion(RegionNames.MainRegion, () => this.container.Resolve<VideosLoadingView>());
            this.regionManager.RegisterViewWithRegion(RegionNames.MainRegion, () => this.container.Resolve<DownloadsView>());
        }

        #endregion Methods
    }
}