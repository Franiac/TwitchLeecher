using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class ErrorDlgVM : DlgBaseVM
    {
        #region Constructors

        public ErrorDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        { }

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
                return "An error occured during the installation";
            }
        }

        public string Message
        {
            get
            {
                return "Your system has not been modified. Please click \"Close\" to exit the Setup Wizard.";
            }
        }

        #endregion Properties
    }
}