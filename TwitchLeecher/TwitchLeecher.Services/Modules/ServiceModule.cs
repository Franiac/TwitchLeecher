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
            this.Bind<ILogService>().To<LogService>().InSingletonScope();
            this.Bind<ITwitchService>().To<TwitchService>().InSingletonScope();
        }

        #endregion Methods
    }
}