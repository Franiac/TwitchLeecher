using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class FinishedDlgVM : DlgBaseVM
    {
        #region Fields

        protected string _headline;

        #endregion Fields

        #region Constructors

        public FinishedDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        {
            if (_bootstrapper.IsUpgrade)
            {
                _headline = "Successfully upgraded " + _bootstrapper.ProductName + " " + _bootstrapper.RelatedBundleVersion.ToString() + " to version " + _bootstrapper.ProductVersionTrimmed.ToString();
            }
            else
            {
                switch (_bootstrapper.LaunchAction)
                {
                    case LaunchAction.Install:
                        _headline = "Successfully installed " + ProductNameVersionDisplay;
                        break;

                    case LaunchAction.Uninstall:
                        _headline = "Successfully uninstalled " + ProductNameVersionDisplay;
                        break;

                    default:
                        throw new ApplicationException("Unsupported LaunchAction '" + _bootstrapper.LaunchAction.ToString() + "'!");
                }
            }
        }

        #endregion Constructors

        #region Properties

        public override string NextButtonText
        {
            get
            {
                return "Finish";
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
                return "Click \"Finish\" to exit the Setup Wizard.";
            }
        }

        #endregion Properties
    }
}