using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class UninstallDlgVM : DlgBaseVM
    {
        #region Fields

        protected IUacService uacService;

        #endregion Fields

        #region Constructors

        public UninstallDlgVM(SetupApplication bootstrapper, IGuiService guiService, IUacService uacService)
            : base(bootstrapper, guiService)
        {
            this.uacService = uacService;
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
                return "Uninstall";
            }
        }

        public bool DeleteUserData
        {
            get
            {
                return this.bootstrapper.DeleteUserData;
            }
            set
            {
                this.bootstrapper.DeleteUserData = value;
                this.FirePropertyChanged("DeleteUserData");
            }
        }

        public override bool IsUacIconVisible
        {
            get
            {
                return this.uacService.IsUacEnabled && !this.uacService.IsUserAdmin;
            }
        }

        #endregion Properties
    }
}