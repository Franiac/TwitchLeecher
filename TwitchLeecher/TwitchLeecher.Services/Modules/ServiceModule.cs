using Ninject.Modules;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Services.Services;

namespace TwitchLeecher.Services.Modules
{
    public class ServiceModule : NinjectModule
    {
        #region Methods

        public override void Load()
        {
            Bind<IUpdateService>().To<UpdateService>().InSingletonScope();
            Bind<IFolderService>().To<FolderService>().InSingletonScope();
            Bind<ILogService>().To<LogService>().InSingletonScope();
            Bind<IPreferencesService>().To<PreferencesService>().InSingletonScope();
            Bind<IRuntimeDataService>().To<RuntimeDataService>().InSingletonScope();
            Bind<ITwitchService>().To<TwitchService>().InSingletonScope();
            Bind<IFilenameService>().To<FilenameService>().InSingletonScope();
        }

        #endregion Methods
    }
}