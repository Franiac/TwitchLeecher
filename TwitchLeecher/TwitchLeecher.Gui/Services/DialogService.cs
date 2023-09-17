using Ninject;
using System;
using Avalonia.Controls;
using Avalonia.Threading;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.ViewModels;
using TwitchLeecher.Gui.Views;
using TwitchLeecher.Services.Interfaces;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;

namespace TwitchLeecher.Gui.Services
{
    internal class DialogService : IDialogService
    {
        #region Fields

        private readonly IKernel _kernel;
        private readonly ILogService _logService;

        private bool _busy;

        #endregion Fields

        #region Constructor

        public DialogService(IKernel kernel, ILogService logService)
        {
            _kernel = kernel;
            _logService = logService;
        }

        #endregion Constructor

        #region Methods

        public MessageBoxResult ShowMessageBox(string message)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message);
            return SetOwnerAndShow(msg);
        }

        public MessageBoxResult ShowMessageBox(string message, string caption)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message, caption);
            return SetOwnerAndShow(msg);
        }

        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message, caption, buttons);
            return SetOwnerAndShow(msg);
        }

        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons,
            MessageBoxImage icon)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message, caption, buttons, icon);
            return SetOwnerAndShow(msg);
        }

        private MessageBoxResult SetOwnerAndShow(MessageBoxWindow msg)
        {
            msg.Owner = Application.Current.MainWindow;
            msg.ShowDialog();

            return msg.Result;
        }

        public void ShowAndLogException(Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            string logFile = _logService.LogException(ex);

            MessageBoxWindow msg = new MessageBoxWindow("An unexpected error occured:"
                                                        + Environment.NewLine + Environment.NewLine + ex.Message
                                                        + Environment.NewLine + Environment.NewLine +
                                                        "All details were written to log file"
                                                        + Environment.NewLine + Environment.NewLine + logFile,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            msg.ShowDialog();
        }

        public void ShowFolderBrowserDialog(string folder, Action<bool, string> dialogCompleteCallback)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                var result = dialog.ShowDialog();

                dialogCompleteCallback(result == DialogResult.OK, dialog.SelectedPath);
            }
        }

        public void ShowFileBrowserDialog(CommonFileDialogFilter filter, string folder,
            Action<bool, string> dialogCompleteCallback)
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Filter = $"{filter.Name}|{filter.Extension}";
                fileDialog.DefaultExt = filter.Extension;

                if (!string.IsNullOrWhiteSpace(folder))
                {
                    fileDialog.InitialDirectory = folder;
                }

                var result = fileDialog.ShowDialog();

                var canceled = result != DialogResult.OK;

                dialogCompleteCallback(canceled, canceled ? null : fileDialog.FileName);
            }
        }

        public void ShowUpdateInfoDialog(UpdateInfo updateInfo)
        {
            UpdateInfoViewVM vm = _kernel.Get<UpdateInfoViewVM>();
            vm.UpdateInfo = updateInfo;

            UpdateInfoView view = new UpdateInfoView
            {
                Owner = Application.Current.MainWindow,
                DataContext = vm
            };

            view.ShowDialog();
        }

        public void SetBusy()
        {
            SetBusy(true);
        }

        private void SetBusy(bool busy)
        {
            if (_busy != busy)
            {
                _busy = busy;

                Mouse.OverrideCursor = busy ? Cursors.Wait : null;

                if (_busy)
                {
                    new DispatcherTimer(TimeSpan.FromSeconds(0), DispatcherPriority.ApplicationIdle,
                        DispatcherTimer_Tick, Dispatcher.CurrentDispatcher);
                }
            }
        }

        #endregion Methods

        #region EventHandlers

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender is DispatcherTimer dispatcherTimer)
            {
                SetBusy(false);
                dispatcherTimer.Stop();
            }
        }

        #endregion EventHandlers
    }
}