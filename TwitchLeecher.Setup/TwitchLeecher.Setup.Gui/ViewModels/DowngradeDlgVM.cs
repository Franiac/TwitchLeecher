using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class DowngradeDlgVM : DlgBaseVM
    {
        #region Fields

        protected string _productName;
        protected string _installedVersion;

        #endregion Fields

        #region Constructors

        public DowngradeDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        {
            _productName = _bootstrapper.ProductName;
            _installedVersion = _bootstrapper.RelatedBundleVersion.ToString();
        }

        #endregion Constructors

        #region Properties

        public override string NextButtonText
        {
            get
            {
                return "Close";
            }
        }

        public override bool IsBackButtonEnabled
        {
            get
            {
                return false;
            }
        }

        public override bool IsCancelButtonEnabled
        {
            get
            {
                return false;
            }
        }

        public string Headline
        {
            get
            {
                return "Installation of " + _productName + " " + _installedVersion + " detected";
            }
        }

        public string Warning
        {
            get
            {
                return "The Setup Wizard has detected a newer version of " + _productName + " on your computer. Please uninstall this version first.";
            }
        }

        #endregion Properties
    }
}