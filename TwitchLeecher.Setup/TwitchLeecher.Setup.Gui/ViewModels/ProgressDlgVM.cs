using TwitchLeecher.Setup.Gui.Services;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class ProgressDlgVM : DlgBaseVM
    {
        #region Fields

        private string headerText;
        private string descText;
        private string statusText;

        private int progressValue;

        #endregion Fields

        #region Constructors

        public ProgressDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        {
            if (this.bootstrapper.IsUpgrade)
            {
                this.headerText = "Upgrading";
                this.descText = "Please wait while the Setup Wizard upgrades " + this.bootstrapper.ProductName + " to version " + this.bootstrapper.ProductVersionTrimmed.ToString();
                this.statusText = "Upgrading...";
            }
            else
            {
                switch (this.bootstrapper.LaunchAction)
                {
                    case LaunchAction.Install:
                        this.headerText = "Installing " + this.ProductNameVersionDisplay;
                        this.descText = "Please wait while the Setup Wizard installs " + this.ProductNameVersionDisplay;
                        this.statusText = "Installing...";
                        break;

                    case LaunchAction.Uninstall:
                        this.headerText = "Uninstalling " + this.ProductNameVersionDisplay;
                        this.descText = "Please wait while the Setup Wizard uninstalls " + this.ProductNameVersionDisplay;
                        this.statusText = "Uninstalling...";
                        break;

                    default:
                        throw new ApplicationException("Unsupported LaunchAction '" + this.bootstrapper.LaunchAction.ToString() + "'!");
                }
            }

            progressValue = 0;
        }

        #endregion Constructors

        #region Properties

        public override bool IsBackButtonEnabled
        {
            get
            {
                return false;
            }
        }

        public override bool IsNextButtonEnabled
        {
            get
            {
                return false;
            }
        }

        public string HeaderText
        {
            get
            {
                return this.headerText;
            }
        }

        public string DescText
        {
            get
            {
                return this.descText;
            }
        }

        public string StatusText
        {
            get
            {
                return this.statusText;
            }
            set
            {
                this.statusText = value;
                this.FirePropertyChanged("StatusText");
            }
        }

        public int ProgressValue
        {
            get
            {
                return this.progressValue;
            }
            set
            {
                this.progressValue = value;
                this.FirePropertyChanged("ProgressValue");
            }
        }

        #endregion Properties
    }
}