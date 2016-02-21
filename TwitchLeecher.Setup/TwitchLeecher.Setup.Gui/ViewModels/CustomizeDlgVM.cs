using TwitchLeecher.Setup.Gui.Services;
using System.IO;
using System.Linq;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class CustomizeDlgVM : DlgBaseVM
    {
        #region Constructors

        public CustomizeDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService) { }

        #endregion Constructors

        #region Properties

        public string InstallDir
        {
            get
            {
                return this.bootstrapper.InstallDir;
            }
            set
            {
                this.bootstrapper.InstallDir = value;
                this.FirePropertyChanged("InstallDir");
            }
        }

        public string FeatureTLSize
        {
            get
            {
                return this.bootstrapper.FeatureTLSize;
            }
        }

        #endregion Properties

        #region Methods

        protected override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = "InstallDir";

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                string installDir = this.bootstrapper.InstallDir;

                bool pathOk = false;
                bool pathEmpty = false;

                if (!string.IsNullOrWhiteSpace(installDir))
                {
                    try
                    {
                        Path.GetFullPath(installDir);

                        if (Path.IsPathRooted(installDir))
                        {
                            pathOk = true;

                            if (Directory.Exists(installDir))
                            {
                                pathEmpty = !Directory.EnumerateFileSystemEntries(installDir).Any();
                            }
                            else
                            {
                                pathEmpty = true;
                            }
                        }
                    }
                    catch
                    {
                        // Parse error
                    }
                }

                if (!pathOk)
                {
                    this.AddError(currentProperty, "Please provide a valid path!");
                }

                if (!pathEmpty)
                {
                    this.AddError(currentProperty, "The specified folder is not empty!");
                }
            }
        }

        #endregion Methods
    }
}