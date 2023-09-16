using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Gui.ViewModels
{
    public class WelcomeViewVM : ViewModelBase
    {
        #region Constructors

        public WelcomeViewVM()
        {
            AssemblyUtil au = AssemblyUtil.Get;

            ProductName = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();
        }

        #endregion Constructors

        #region Properties

        public string ProductName { get; }

        #endregion Properties
    }
}