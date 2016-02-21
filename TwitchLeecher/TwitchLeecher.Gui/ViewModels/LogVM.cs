using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Services;

namespace TwitchLeecher.Gui.ViewModels
{
    public class LogVM : BaseVM
    {
        #region Fields

        private TwitchVideoDownload download;

        private ICommand closeCommand;

        private IGuiService guiService;

        private object searchLock = new object();

        #endregion Fields

        #region Constructors

        public LogVM(IGuiService guiService)
        {
            this.guiService = guiService;
        }

        #endregion Constructors

        #region Properties

        public TwitchVideoDownload Download
        {
            get
            {
                return this.download;
            }
            set
            {
                this.SetProperty(ref this.download, value, nameof(this.Download));
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (this.closeCommand == null)
                {
                    this.closeCommand = new DelegateCommand<Window>(this.Close);
                }

                return this.closeCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void Close(Window window)
        {
            try
            {
                window.DialogResult = false;
                window.Close();
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        #endregion Methods
    }
}