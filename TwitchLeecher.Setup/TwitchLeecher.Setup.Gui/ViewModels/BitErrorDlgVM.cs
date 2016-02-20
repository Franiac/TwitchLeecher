using TwitchLeecher.Setup.Gui.Services;
using System;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class BitErrorDlgVM : DlgBaseVM
    {
        #region Fields

        protected string installerBit;
        protected string osBit;

        #endregion Fields

        #region Constructors

        public BitErrorDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        {
            this.installerBit = this.bootstrapper.IsBundle64Bit ? "64" : "32";
            this.osBit = Environment.Is64BitOperatingSystem ? "64" : "32";
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
                return "Invalid installer architecture for this operating system";
            }
        }

        public string Warning
        {
            get
            {
                return "The " + this.installerBit + " Bit installer cannot be used on a " + osBit + " Bit operating system. Please use the " + osBit + " Bit installer instead.";
            }
        }

        #endregion Properties
    }
}