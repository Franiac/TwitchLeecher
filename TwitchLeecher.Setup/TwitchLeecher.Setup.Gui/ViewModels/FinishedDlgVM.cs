using TwitchLeecher.Setup.Gui.Services;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class FinishedDlgVM : DlgBaseVM
    {
        #region Fields

        protected string headline;

        #endregion Fields

        #region Constructors

        public FinishedDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        {
            if (this.bootstrapper.IsUpgrade)
            {
                this.headline = "Successfully upgraded " + this.bootstrapper.ProductName + " " + this.bootstrapper.RelatedBundleVersion.ToString() + " to version " + this.bootstrapper.ProductVersionTrimmed.ToString();
            }
            else
            {
                switch (this.bootstrapper.LaunchAction)
                {
                    case LaunchAction.Install:
                        this.headline = "Successfully installed " + this.ProductNameVersionDisplay;
                        break;

                    case LaunchAction.Uninstall:
                        this.headline = "Successfully uninstalled " + this.ProductNameVersionDisplay;
                        break;

                    default:
                        throw new ApplicationException("Unsupported LaunchAction '" + this.bootstrapper.LaunchAction.ToString() + "'!");
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
                return this.headline;
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