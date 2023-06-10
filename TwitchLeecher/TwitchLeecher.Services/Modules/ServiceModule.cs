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
            Bind<IApiService>().To<ApiService>().InSingletonScope();
            Bind<IAuthService>().To<AuthService>().InSingletonScope();
            Bind<IDownloadService>().To<DownloadService>().InSingletonScope();
            Bind<IFilenameService>().To<FilenameService>().InSingletonScope();
            Bind<IFolderService>().To<FolderService>().InSingletonScope();
            Bind<ILogService>().To<LogService>().InSingletonScope();
            Bind<IPreferencesService>().To<PreferencesService>().InSingletonScope();
            Bind<IProcessingService>().To<ProcessingService>().InSingletonScope();
            Bind<IRuntimeDataService>().To<RuntimeDataService>().InSingletonScope();
            Bind<IUpdateService>().To<UpdateService>().InSingletonScope();
            Bind<IAuthListener>().To<AuthListener>().InSingletonScope();
            Bind<IThemeService>().To<ThemeService>().InSingletonScope();
            Bind<ICookieService>().To<CookieService>().InSingletonScope();
        }

        #endregion Methods
    }
}