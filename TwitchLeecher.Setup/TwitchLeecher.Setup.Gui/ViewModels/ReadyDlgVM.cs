using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class ReadyDlgVM : DlgBaseVM
    {
        #region Fields

        protected IUacService _uacService;

        #endregion Fields

        #region Constructors

        public ReadyDlgVM(SetupApplication bootstrapper, IGuiService guiService, IUacService uacService)
            : base(bootstrapper, guiService)
        {
            _uacService = uacService;
        }

        #endregion Constructors

        #region Properties

        public override string NextButtonText
        {
            get
            {
                return "Install";
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