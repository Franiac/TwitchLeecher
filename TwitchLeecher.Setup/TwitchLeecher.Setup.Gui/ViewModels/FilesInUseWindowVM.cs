using System;
using System.Collections.Generic;
using System.Windows.Input;
using TwitchLeecher.Setup.Gui.Command;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class FilesInUseWindowVM
    {
        #region Fields

        private IList<string> files;

        private ICommand retryCommand;
        private ICommand cancelCommand;

        private Action windowCloseAction;
        private Action<bool?> setDialogResultAction;

        #endregion Fields

        #region Constructors

        public FilesInUseWindowVM(IList<string> files)
        {
            if (files == null || files.Count == 0)
            {
                throw new ArgumentNullException("files");
            }

            this.files = files;
        }

        #endregion Constructors

        #region Properties

        public IList<string> Files
        {
            get
            {
                return this.files;
            }
        }

        public ICommand RetryCommand
        {
            get
            {
                if (this.retryCommand == null)
                {
                    this.retryCommand = new DelegateCommand(this.Retry);
                }

                return this.retryCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                {
                    this.cancelCommand = new DelegateCommand(this.Cancel);
                }

                return this.cancelCommand;
            }
        }

        #endregion Properties

        #region Methods

        public void SetWindowCloseAction(Action windowCloseAction)
        {
            if (windowCloseAction == null)
            {
                throw new ArgumentNullException("windowCloseAction");
            }

            this.windowCloseAction = windowCloseAction;
        }

        public void SetSetDialogResultAction(Action<bool?> setDialogResultAction)
        {
            if (setDialogResultAction == null)
            {
                throw new ArgumentNullException("setDialogResultAction");
            }

            this.setDialogResultAction = setDialogResultAction;
        }

        private void Retry()
        {
            this.setDialogResultAction(true);
            this.windowCloseAction();
        }

        private void Cancel()
        {
            this.windowCloseAction();
        }

        #endregion Methods
    }
}