using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Unity;
using System.Windows;
using TwitchLeecher.Gui.Modules;
using TwitchLeecher.Services.Modules;

namespace TwitchLeecher
{
    public class Bootstrapper : UnityBootstrapper
    {
        #region Methods

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();

            ModuleCatalog mc = (ModuleCatalog)this.ModuleCatalog;

            mc.AddModule(typeof(GuiModule), nameof(ServiceModule));
            mc.AddModule(typeof(ServiceModule));
        }

        protected override DependencyObject CreateShell()
        {
            return this.Container.Resolve<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            Application.Current.MainWindow = (Window)this.Shell;
            Application.Current.MainWindow.Show();
        }

        #endregion Methods
    }
}