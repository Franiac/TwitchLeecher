using Prism.Mvvm;
using TwitchLeecher.Common;

namespace TwitchLeecher.Gui.ViewModels
{
    public class WelcomeVM : BindableBase
    {
        #region Fields

        private string productName;

        #endregion Fields

        #region Constructors

        public WelcomeVM()
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