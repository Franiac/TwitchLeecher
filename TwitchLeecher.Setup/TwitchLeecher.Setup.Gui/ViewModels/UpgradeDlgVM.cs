using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class UpgradeDlgVM : DlgBaseVM
    {
        #region Fields

        protected string _productName;
        protected string _oldVersion;
        protected string _newVersion;

        protected IUacService _uacService;

        #endregion Fields

        #region Constructors

        public UpgradeDlgVM(SetupApplication bootstrapper, IGuiService guiService, IUacService uacService)
            : base(bootstrapper, guiService)
        {
            _uacService = uacService;

            _productName = _bootstrapper.ProductName;
            _oldVersion = _bootstrapper.RelatedBundleVersion.ToString();
            _newVersion = _bootstrapper.ProductVersionTrimmed.ToString();
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
                return "Upgrade " + _productName + " " + _oldVersion + " to version " + _newVersion;
            }
        }

        public string Message
        {
            get
            {
                return "The Setup Wizard will upgrade your current installation of " + _productName + " " + _oldVersion + " to version " + _newVersion + ". Click \"Upgrade\" to continue or \"Cancel\" to exit the Setup Wizard.";
            }
        }

        public override bool IsUacIconVisible
        {
            get
            {
                return _uacService.IsUacEnabled && !_uacService.IsUserAdmin;
            }
        }

        #endregion Properties
    }
}