using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class ProgressDlgVM : DlgBaseVM
    {
        #region Fields

        private string _statusText;

        private int progressValue;

        #endregion Fields

        #region Constructors

        public ProgressDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        {
            if (_bootstrapper.IsUpgrade)
            {
                HeaderText = "Upgrading";
                DescText = "Please wait while the Setup Wizard upgrades " + _bootstrapper.ProductName + " to version " + _bootstrapper.ProductVersionTrimmed.ToString();
                _statusText = "Upgrading...";
            }
            else
            {
                switch (_bootstrapper.LaunchAction)
                {
                    case LaunchAction.Install:
                        HeaderText = "Installing " + ProductNameVersionDisplay;
                        DescText = "Please wait while the Setup Wizard installs " + ProductNameVersionDisplay;
                        _statusText = "Installing...";
                        break;

                    case LaunchAction.Uninstall:
                        HeaderText = "Uninstalling " + ProductNameVersionDisplay;
                        DescText = "Please wait while the Setup Wizard uninstalls " + ProductNameVersionDisplay;
                        _statusText = "Uninstalling...";
                        break;

                    default:
                        throw new ApplicationException("Unsupported LaunchAction '" + _bootstrapper.LaunchAction.ToString() + "'!");
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

        public string HeaderText { get; }

        public string DescText { get; }

        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
                FirePropertyChanged("StatusText");
            }
        }

        public int ProgressValue
        {
            get
            {
                return progressValue;
            }
            set
            {
                progressValue = value;
                FirePropertyChanged("ProgressValue");
            }
        }

        #endregion Properties
    }
}