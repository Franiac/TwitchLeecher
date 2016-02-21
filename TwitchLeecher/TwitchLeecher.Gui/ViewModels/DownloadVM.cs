using Prism.Commands;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Services;

namespace TwitchLeecher.Gui.ViewModels
{
    public class DownloadVM : DialogVM<DownloadParameters>
    {
        #region Fields

        private TwitchVideo video;
        private TwitchVideoResolution resolution;

        private string filename;

        private ICommand chooseCommand;
        private ICommand downloadCommand;
        private ICommand cancelCommand;

        private IGuiService guiService;

        private object searchLock = new object();

        #endregion Fields

        #region Constructors

        public DownloadVM(IGuiService guiService)
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
                this.Resolution = this.video.Resolutions.FirstOrDefault();
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
                this.guiService.ShowSaveFileDialog(this.filename, this.ChooseCallback);
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        private void ChooseCallback(bool cancelled, string filename)
        {
            try
            {
                if (!cancelled)
                {
                    this.Filename = filename;
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
                    this.ResultObject = new DownloadParameters(this.video, this.resolution, this.filename);
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

        protected override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(this.Filename);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(this.filename))
                {
                    this.AddError(currentProperty, "Please specify a filename!");
                }
            }
        }

        #endregion Methods
    }
}