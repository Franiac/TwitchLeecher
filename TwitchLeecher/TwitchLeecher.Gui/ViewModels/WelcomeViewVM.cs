using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Gui.ViewModels
{
    public class WelcomeViewVM : ViewModelBase
    {
        #region Fields

        private string _productName;

        #endregion Fields

        #region Constructors

        public WelcomeViewVM()
        {
            AssemblyUtil au = AssemblyUtil.Get;

            _productName = au.GetProductName() + " " + au.GetAssemblyVersion().Trim();
        }

        #endregion Constructors

        #region Properties

        public string ProductName
        {
            get
            {
                return _productName;
            }
        }

        #endregion Properties
    }
}