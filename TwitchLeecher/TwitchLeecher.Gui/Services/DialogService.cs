using Ninject;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.Views;
using TwitchLeecher.Services.Interfaces;
using Cursors = System.Windows.Input.Cursors;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace TwitchLeecher.Gui.Services
{
    internal class DialogService : IDialogService
    {
        #region Fields

        private IKernel kernel;
        private ILogService logService;

        private bool busy;

        #endregion Fields

        #region Constructor

        public DialogService(IKernel kernel, ILogService logService)
        {
            this.kernel = kernel;
            this.logService = logService;
        }

        #endregion Constructor

        #region Methods

        public MessageBoxResult ShowMessageBox(string message)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message);
            msg.ShowDialog();

            return msg.Result;
        }

        public MessageBoxResult ShowMessageBox(string message, string caption)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message, caption);
            msg.ShowDialog();

            return msg.Result;
        }

        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message, caption, buttons);
            msg.ShowDialog();

            return msg.Result;
        }

        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message, caption, buttons, icon);
            msg.ShowDialog();

            return msg.Result;
        }

        public void ShowAndLogException(Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            ILogService logService = this.kernel.Get<ILogService>();
            string logFile = logService.LogException(ex);

            MessageBoxWindow msg = new MessageBoxWindow("An unexpected error occured:"
                + Environment.NewLine + Environment.NewLine + ex.Message
                + Environment.NewLine + Environment.NewLine + "All details were written to log file"
                + Environment.NewLine + Environment.NewLine + logFile,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            msg.ShowDialog();
        }

        public void ShowFolderBrowserDialog(string folder, Action<bool, string> dialogCompleteCallback)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (!string.IsNullOrWhiteSpace(folder))
                {
                    fbd.SelectedPath = folder;
                }

                DialogResult result = fbd.ShowDialog();

                dialogCompleteCallback(result != DialogResult.OK, fbd.SelectedPath);
            }
        }

        public void ShowSaveFileDialog(string filename, Action<bool, string> dialogCompleteCallback)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Broadcasts|*.mp4";

            if (!string.IsNullOrWhiteSpace(filename))
            {
                sfd.FileName = filename;
            }

            bool? result = sfd.ShowDialog();

            dialogCompleteCallback(result != true, sfd.FileName);
        }

        public void SetBusy()
        {
            this.SetBusy(true);
        }

        private void SetBusy(bool busy)
        {
            if (this.busy != busy)
            {
                this.busy = busy;

                Mouse.OverrideCursor = busy ? Cursors.Wait : null;

                if (this.busy)
                {
                    new DispatcherTimer(TimeSpan.FromSeconds(0), DispatcherPriority.ApplicationIdle, dispatcherTimer_Tick, Dispatcher.CurrentDispatcher);
                }
            }
        }

        #endregion Methods

        #region EventHandlers

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer dispatcherTimer = sender as DispatcherTimer;

            if (dispatcherTimer != null)
            {
                SetBusy(false);
                dispatcherTimer.Stop();
            }
        }

        #endregion EventHandlers
    }
}