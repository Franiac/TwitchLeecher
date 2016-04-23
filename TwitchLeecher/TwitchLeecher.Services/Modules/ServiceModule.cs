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
            this.Bind<IUpdateService>().To<UpdateService>().InSingletonScope();
            this.Bind<IFolderService>().To<FolderService>().InSingletonScope();
            this.Bind<ILogService>().To<LogService>().InSingletonScope();
            this.Bind<IPreferencesService>().To<PreferencesService>().InSingletonScope();
            this.Bind<ITwitchService>().To<TwitchService>().InSingletonScope();
            this.Bind<IFilenameService>().To<FilenameService>().InSingletonScope();
        }

        #endregion Methods
    }
}