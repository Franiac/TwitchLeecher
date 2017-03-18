using System.IO;
using System.Linq;
using TwitchLeecher.Setup.Gui.Services;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class CustomizeDlgVM : DlgBaseVM
    {
        #region Constructors

        public CustomizeDlgVM(SetupApplication bootstrapper, IGuiService guiService)
            : base(bootstrapper, guiService)
        { }

        #endregion Constructors

        #region Properties

        public string InstallDir
        {
            get
            {
                return _bootstrapper.InstallDir;
            }
            set
            {
                _bootstrapper.InstallDir = value;
                FirePropertyChanged("InstallDir");
            }
        }

        public string FeatureTLSize
        {
            get
            {
                return _bootstrapper.FeatureTLSize;
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
                string installDir = _bootstrapper.InstallDir;

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
                    AddError(currentProperty, "Please provide a valid path!");
                }

                if (!pathEmpty)
                {
                    AddError(currentProperty, "The specified folder is not empty!");
                }
            }
        }

        #endregion Methods
    }
}