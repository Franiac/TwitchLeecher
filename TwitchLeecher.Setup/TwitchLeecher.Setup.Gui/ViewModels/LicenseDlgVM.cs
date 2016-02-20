using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class LicenseDlgVM : DlgBaseVM
    {
        #region Constructors

        public LicenseDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService) { }

        #endregion Constructors

        #region Properties

        public bool LicenseAccepted
        {
            get
            {
                return this.bootstrapper.LicenseAccepted;
            }
            set
            {
                this.bootstrapper.LicenseAccepted = value;
                this.FirePropertyChanged("LicenseAccepted");
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
                if (!this.bootstrapper.LicenseAccepted)
                {
                    this.AddError(currentProperty, "Please accept the license agreement!");
                }
            }
        }

        #endregion Methods
    }
}