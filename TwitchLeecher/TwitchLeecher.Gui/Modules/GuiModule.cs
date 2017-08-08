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
            Bind<IDialogService>().To<DialogService>().InSingletonScope();
            Bind<IDonationService>().To<DonationService>().InSingletonScope();
            Bind<INavigationService>().To<NavigationService>().InSingletonScope();
            Bind<INotificationService>().To<NotificationService>().InSingletonScope();
            Bind<ISearchService>().To<SearchService>().InSingletonScope();
        }

        #endregion Methods
    }
}