using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Gui.ViewModels
{
    public class WelcomeViewVM : ViewModelBase
    {
        #region Fields

        private string productName;

        #endregion Fields

        #region Constructors

        public WelcomeViewVM()
        {
            AssemblyUtil au = AssemblyUtil.Get;

            this.productName = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();
        }

        #endregion Constructors

        #region Properties

        public string ProductName
        {
            get
            {
                return this.productName;
            }
        }

        #endregion Properties
    }
}