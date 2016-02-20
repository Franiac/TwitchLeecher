using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class ReadyDlgVM : DlgBaseVM
    {
        #region Constructors

        public ReadyDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService) { }

        #endregion Constructors

        #region Properties

        public override string NextButtonText
        {
            get
            {
                return "Install";
            }
        }

        #endregion Properties
    }
}