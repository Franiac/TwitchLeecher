using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class UninstallDlgVM : DlgBaseVM
    {
        #region Fields

        protected IUacService _uacService;

        #endregion Fields

        #region Constructors

        public UninstallDlgVM(SetupApplication bootstrapper, IGuiService guiService, IUacService uacService)
            : base(bootstrapper, guiService)
        {
            _uacService = uacService;
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
                return _bootstrapper.DeleteUserData;
            }
            set
            {
                _bootstrapper.DeleteUserData = value;
                FirePropertyChanged("DeleteUserData");
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