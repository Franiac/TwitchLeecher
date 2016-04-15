using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class ReadyDlgVM : DlgBaseVM
    {
        #region Fields

        protected IUacService uacService;

        #endregion Fields

        #region Constructors

        public ReadyDlgVM(SetupApplication bootstrapper, IGuiService guiService, IUacService uacService)
            : base(bootstrapper, guiService)
        {
            this.uacService = uacService;
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