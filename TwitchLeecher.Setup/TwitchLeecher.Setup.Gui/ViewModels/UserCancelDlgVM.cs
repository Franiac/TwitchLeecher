using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class UserCancelDlgVM : DlgBaseVM
    {
        #region Fields

        protected string _headline;

        #endregion Fields

        #region Constructors

        public UserCancelDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        {
            if (_bootstrapper.IsUpgrade)
            {
                _headline = "Upgrade";
            }
            else
            {
                switch (_bootstrapper.LaunchAction)
                {
                    case LaunchAction.Install:
                        _headline = "Installation";
                        break;

                    case LaunchAction.Uninstall:
                        _headline = "Uninstallation";
                        break;

                    default:
                        throw new ApplicationException("Unsupported LaunchAction '" + _bootstrapper.LaunchAction.ToString() + "'!");
                }
            }

            _headline += " has been cancelled";
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
                return _headline;
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