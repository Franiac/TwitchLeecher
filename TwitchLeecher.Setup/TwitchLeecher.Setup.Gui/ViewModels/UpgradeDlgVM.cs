using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class UpgradeDlgVM : DlgBaseVM
    {
        #region Fields

        protected string productName;
        protected string oldVersion;
        protected string newVersion;

        protected IUacService uacService;

        #endregion Fields

        #region Constructors

        public UpgradeDlgVM(SetupApplication bootstrapper, IGuiService guiService, IUacService uacService)
            : base(bootstrapper, guiService)
        {
            this.uacService = uacService;

            this.productName = this.bootstrapper.ProductName;
            this.oldVersion = this.bootstrapper.RelatedBundleVersion.ToString();
            this.newVersion = this.bootstrapper.ProductVersionTrimmed.ToString();
        }

        #endregion Constructors

        #region Properties

        public override bool IsBackButtonEnabled
        {
            get
            {
                return false;
            }
        }

        public override string NextButtonText
        {
            get
            {
                return "Upgrade";
            }
        }

        public string Headline
        {
            get
            {
                return "Upgrade " + this.productName + " " + this.oldVersion + " to version " + this.newVersion;
            }
        }

        public string Message
        {
            get
            {
                return "The Setup Wizard will upgrade your current installation of " + this.productName + " " + this.oldVersion + " to version " + this.newVersion + ". Click \"Upgrade\" to continue or \"Cancel\" to exit the Setup Wizard.";
            }
        }

        public override bool IsUacShieldVisible
        {
            get
            {
                return this.uacService.IsUacEnabled && !this.uacService.IsUserAdmin;
            }
        }

        #endregion Properties
    }
}