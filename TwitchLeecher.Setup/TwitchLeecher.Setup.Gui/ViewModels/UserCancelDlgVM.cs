using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class UserCancelDlgVM : DlgBaseVM
    {
        #region Fields

        protected string headline;

        #endregion Fields

        #region Constructors

        public UserCancelDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        {
            if (this.bootstrapper.IsUpgrade)
            {
                this.headline = "Upgrade";
            }
            else
            {
                switch (this.bootstrapper.LaunchAction)
                {
                    case LaunchAction.Install:
                        this.headline = "Installation";
                        break;

                    case LaunchAction.Uninstall:
                        this.headline = "Uninstallation";
                        break;

                    default:
                        throw new ApplicationException("Unsupported LaunchAction '" + this.bootstrapper.LaunchAction.ToString() + "'!");
                }
            }

            headline += " has been cancelled";
        }

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
                return this.headline;
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