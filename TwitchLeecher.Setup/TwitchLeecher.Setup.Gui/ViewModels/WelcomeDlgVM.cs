using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class WelcomeDlgVM : DlgBaseVM
    {
        #region Constructors

        public WelcomeDlgVM(SetupApplication bootstrapper, IGuiService guiService)
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

        public string Headline
        {
            get
            {
                return "Welcome to " + ProductNameVersionDisplay + " Setup Wizard";
            }
        }

        public string Message
        {
            get
            {
                return "The Setup Wizard will install " + ProductNameVersionDisplay + " on your computer. Click \"Next\" to continue or \"Cancel\" to exit the Setup Wizard.";
            }
        }

        #endregion Properties
    }
}