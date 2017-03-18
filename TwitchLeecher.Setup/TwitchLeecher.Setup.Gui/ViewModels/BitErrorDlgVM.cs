using System;
using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class BitErrorDlgVM : DlgBaseVM
    {
        #region Fields

        protected string _installerBit;
        protected string _osBit;

        #endregion Fields

        #region Constructors

        public BitErrorDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        {
            _installerBit = _bootstrapper.IsBundle64Bit ? "64" : "32";
            _osBit = Environment.Is64BitOperatingSystem ? "64" : "32";
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
                return "The " + _installerBit + " Bit installer cannot be used on a " + _osBit + " Bit operating system. Please use the " + _osBit + " Bit installer instead.";
            }
        }

        #endregion Properties
    }
}