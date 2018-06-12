using System;
using System.Collections.Generic;
using System.Windows.Input;
using TwitchLeecher.Setup.Gui.Command;

namespace TwitchLeecher.Setup.Gui.ViewModels
{
    internal class FilesInUseWindowVM
    {
        #region Fields

        private ICommand _retryCommand;
        private ICommand _cancelCommand;

        private Action _windowCloseAction;
        private Action<bool?> _setDialogResultAction;

        #endregion Fields

        #region Constructors

        public FilesInUseWindowVM(IList<string> files)
        {
            if (files == null || files.Count == 0)
            {
                throw new ArgumentNullException("files");
            }

            Files = files;
        }

        #endregion Constructors

        #region Properties

        public IList<string> Files { get; }

        public ICommand RetryCommand
        {
            get
            {
                if (_retryCommand == null)
                {
                    _retryCommand = new DelegateCommand(Retry);
                }

                return _retryCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new DelegateCommand(Cancel);
                }

                return _cancelCommand;
            }
        }

        #endregion Properties

        #region Methods

        public void SetWindowCloseAction(Action windowCloseAction)
        {
            _windowCloseAction = windowCloseAction ?? throw new ArgumentNullException("windowCloseAction");
        }

        public void SetSetDialogResultAction(Action<bool?> setDialogResultAction)
        {
            _setDialogResultAction = setDialogResultAction ?? throw new ArgumentNullException("setDialogResultAction");
        }

        private void Retry()
        {
            _setDialogResultAction(true);
            _windowCloseAction();
        }

        private void Cancel()
        {
            _windowCloseAction();
        }

        #endregion Methods
    }
}