using Ninject.Modules;
using TwitchLeecher.Gui.Services;

namespace TwitchLeecher.Gui.Modules
{
    public class GuiModule : NinjectModule
    {
        #region Methods

        public override void Load()
        {
            this.Bind<IGuiService>().To<GuiService>().InSingletonScope();
        }

        #endregion Methods
    }
}