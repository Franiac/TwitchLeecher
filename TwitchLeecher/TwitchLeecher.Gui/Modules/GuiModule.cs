using Ninject.Modules;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.Services;

namespace TwitchLeecher.Gui.Modules
{
    public class GuiModule : NinjectModule
    {
        #region Methods

        public override void Load()
        {
            this.Bind<IDialogService>().To<DialogService>().InSingletonScope();
            this.Bind<INavigationService>().To<NavigationService>().InSingletonScope();
            this.Bind<INotificationService>().To<NotificationService>().InSingletonScope();
            this.Bind<ISearchService>().To<SearchService>().InSingletonScope();
        }

        #endregion Methods
    }
}