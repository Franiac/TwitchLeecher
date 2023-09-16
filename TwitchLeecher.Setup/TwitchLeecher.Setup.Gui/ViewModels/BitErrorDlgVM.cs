using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class BitErrorDlgVM : DlgBaseVM
    {
        #region Constructors

        public BitErrorDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService) { }

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
                return "Incompatible Operating System";
            }
        }

        public string Warning
        {
            get
            {
                return "Twitch Leecher requires a 64 bit operating system!";
            }
        }

        #endregion Properties
    }
}