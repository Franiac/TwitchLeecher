using Ninject.Modules;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Services.Services;
using TwitchLeecher.Services.Services.Download;
using TwitchLeecher.Services.Services.Processing;

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
            this.Bind<IRuntimeDataService>().To<RuntimeDataService>().InSingletonScope();
            this.Bind<ITwitchService>().To<TwitchService>().InSingletonScope();
            this.Bind<IFilenameService>().To<FilenameService>().InSingletonScope();
            this.Bind<IDownloadService>().To<DownloadService>().InSingletonScope();
            this.Bind<IProcessingService>().To<ProcessingService>().InSingletonScope();
        }

        #endregion Methods
    }
}