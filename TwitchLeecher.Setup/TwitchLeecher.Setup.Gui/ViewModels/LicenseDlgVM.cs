using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class LicenseDlgVM : DlgBaseVM
    {
        #region Constructors

        public LicenseDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        { }

        #endregion Constructors

        #region Properties

        public bool LicenseAccepted
        {
            get
            {
                return _bootstrapper.LicenseAccepted;
            }
            set
            {
                _bootstrapper.LicenseAccepted = value;
                FirePropertyChanged("LicenseAccepted");
            }
        }

        #endregion Properties

        #region Methods

        protected override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = "LicenseAccepted";

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (!_bootstrapper.LicenseAccepted)
                {
                    AddError(currentProperty, "Please accept the license agreement!");
                }
            }
        }

        #endregion Methods
    }
}