using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class UninstallDlgVM : DlgBaseVM
    {
        #region Constructors

        public UninstallDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        { }

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

        #endregion Properties
    }
}