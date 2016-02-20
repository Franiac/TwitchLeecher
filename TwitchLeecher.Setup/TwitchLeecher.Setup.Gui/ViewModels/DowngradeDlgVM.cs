using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class DowngradeDlgVM : DlgBaseVM
    {
        #region Fields

        protected string productName;
        protected string installedVersion;

        #endregion Fields

        #region Constructors

        public DowngradeDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        {
            this.productName = this.bootstrapper.ProductName;
            this.installedVersion = this.bootstrapper.RelatedBundleVersion.ToString();
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
                return "Installation of " + this.productName + " " + this.installedVersion + " detected";
            }
        }

        public string Warning
        {
            get
            {
                return "The Setup Wizard has detected a newer version of " + this.productName + " on your computer. Please uninstall this version first.";
            }
        }

        #endregion Properties
    }
}