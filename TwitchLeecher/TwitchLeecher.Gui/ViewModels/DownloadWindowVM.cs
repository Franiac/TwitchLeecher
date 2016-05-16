using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Services;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels
{
    public class DownloadWindowVM : DialogVM<DownloadParameters>
    {
        #region Fields

        private DownloadParameters downloadParams;

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

        public DownloadParameters DownloadParams
        {
            get
            {
                return this.downloadParams;
            }
            set
            {
                this.SetProperty(ref this.downloadParams, value, nameof(this.DownloadParams));
            }
        }

        public string CropStartTime
        {
            get
            {
                return this.downloadParams.CropStartTime.ToString();
            }
            set
            {
                TimeSpan ts;

                if (TimeSpan.TryParse(value, out ts))
                {
                    this.downloadParams.CropStartTime = ts;
                }

                this.FirePropertyChanged(nameof(this.CropStartTime));
            }
        }

        public string CropEndTime
        {
            get
            {
                return this.downloadParams.CropEndTime.ToString();
            }
            set
            {
                TimeSpan ts;

                if (TimeSpan.TryParse(value, out ts))
                {
                    this.downloadParams.CropEndTime = ts;
                }

                this.FirePropertyChanged(nameof(this.CropEndTime));
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
                this.guiService.ShowFolderBrowserDialog(this.downloadParams.Folder, this.ChooseCallback);
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
                    this.downloadParams.Folder = folder;
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
                    if (File.Exists(this.downloadParams.FullPath))
                    {
                        MessageBoxResult result = this.guiService.ShowMessageBox("The file already exists. Do you want to overwrite it?", "Download", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result != MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }

                    this.ResultObject = this.downloadParams;
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

            string currentProperty = nameof(this.DownloadParams);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                this.DownloadParams?.Validate();

                if (this.DownloadParams.HasErrors)
                {
                    this.AddError(currentProperty, "Invalid Download Parameters!");
                }
            }

            currentProperty = nameof(this.CropStartTime);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                this.DownloadParams?.Validate(currentProperty);

                if (this.DownloadParams.HasErrors)
                {
                    List<string> errors = this.DownloadParams.GetErrors(currentProperty) as List<string>;

                    if (errors != null && errors.Count > 0)
                    {
                        this.AddError(currentProperty, errors.First());
                    }
                }
            }

            currentProperty = nameof(this.CropEndTime);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                this.DownloadParams?.Validate(currentProperty);

                if (this.DownloadParams.HasErrors)
                {
                    List<string> errors = this.DownloadParams.GetErrors(currentProperty) as List<string>;

                    if (errors != null && errors.Count > 0)
                    {
                        this.AddError(currentProperty, errors.First());
                    }
                }
            }
        }

        #endregion Methods
    }
}