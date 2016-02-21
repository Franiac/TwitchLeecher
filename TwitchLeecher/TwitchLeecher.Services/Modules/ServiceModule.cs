using Microsoft.Practices.Unity;
using Prism.Modularity;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Services.Services;

namespace TwitchLeecher.Services.Modules
{
    public class ServiceModule : IModule
    {
        #region Fields

        private IUnityContainer container;

        #endregion Fields

        #region Constructors

        public ServiceModule(IUnityContainer container)
        {
            this.container = container;
        }

        #endregion Constructors

        #region Methods

        public void Initialize()
        {
            this.container.RegisterType<ILogService, LogService>(new ContainerControlledLifetimeManager());
            this.container.RegisterType<ITwitchService, TwitchService>(new ContainerControlledLifetimeManager());
        }

        #endregion Methods
    }
}