using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Services;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.IO;

namespace TwitchLeecher.Gui.ViewModels
{
    public class DownloadWindowVM : DialogVM<DownloadParameters>
    {
        #region Fields

        private TwitchVideo video;
        private TwitchVideoResolution resolution;

        private string folder;
        private string filename;

        private ICommand chooseCommand;
        private ICommand downloadCommand;
        private ICommand cancelCommand;

        private IGuiService guiService;

        private object searchLock = new object();

        #endregion Fields

        #region Constructors

        public DownloadWindowVM(IGuiService guiService)
        {
            this.guiService = guiService;
        }

        #endregion Constructors

        #region Properties

        public TwitchVideo Video
        {
            get
            {
                return this.video;
            }
            set
            {
                this.SetProperty(ref this.video, value, nameof(this.Video));
            }
        }

        public TwitchVideoResolution Resolution
        {
            get
            {
                return this.resolution;
            }
            set
            {
                this.SetProperty(ref this.resolution, value, nameof(this.Resolution));
            }
        }

        public string Folder
        {
            get
            {
                return this.folder;
            }
            set
            {
                this.SetProperty(ref this.folder, value, nameof(this.Folder));
            }
        }

        public string Filename
        {
            get
            {
                return this.filename;
            }
            set
            {
                this.SetProperty(ref this.filename, value, nameof(this.Filename));
            }
        }

        public ICommand ChooseCommand
        {
            get
            {
                if (this.chooseCommand == null)
                {
                    this.chooseCommand = new DelegateCommand(this.Choose);
                }

                return this.chooseCommand;
            }
        }

        public ICommand DownloadCommand
        {
            get
            {
                if (this.downloadCommand == null)
                {
                    this.downloadCommand = new DelegateCommand<Window>(this.Download);
                }

                return this.downloadCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                {
                    this.cancelCommand = new DelegateCommand<Window>(this.Cancel);
                }

                return this.cancelCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Choose()
        {
            try
            {
                this.guiService.ShowFolderBrowserDialog(this.folder, this.ChooseCallback);
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        private void ChooseCallback(bool cancelled, string folder)
        {
            try
            {
                if (!cancelled)
                {
                    this.Folder = folder;
                }
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        private void Download(Window window)
        {
            try
            {
                this.Validate();

                if (!this.HasErrors)
                {
                    string filename = Path.Combine(this.folder, this.filename);

                    if (File.Exists(filename))
                    {
                        MessageBoxResult result = this.guiService.ShowMessageBox("The file already exists. Do you want to overwrite it?", "Download", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result != MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }

                    this.ResultObject = new DownloadParameters(this.video, this.resolution, filename);
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        private void Cancel(Window window)
        {
            try
            {
                this.ResultObject = null;
                window.DialogResult = false;
                window.Close();
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(this.Resolution);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (this.resolution == null)
                {
                    this.AddError(currentProperty, "Please select a quality!");
                }
            }

            currentProperty = nameof(this.Folder);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(this.folder))
                {
                    this.AddError(currentProperty, "Please specify a folder!");
                }
                else if (!Directory.Exists(this.folder))
                {
                    this.AddError(currentProperty, "The specified folder does not exist!");
                }
                else if (!FileSystem.HasWritePermission(this.folder))
                {
                    this.AddError(currentProperty, "You do not have write permissions on the specified folder! Please choose a different one!");
                }
            }

            currentProperty = nameof(this.Filename);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(this.filename))
                {
                    this.AddError(currentProperty, "Please specify a filename!");
                }
                else if (!this.filename.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    this.AddError(currentProperty, "Filename must end with '.mp4'!");
                }
                else if (FileSystem.FilenameContainsInvalidChars(this.filename))
                {
                    this.AddError(currentProperty, "Filename contains invalid characters!");
                }
            }
        }

        #endregion Methods
    }
}