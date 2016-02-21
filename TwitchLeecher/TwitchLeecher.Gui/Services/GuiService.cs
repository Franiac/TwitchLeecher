using Microsoft.Practices.Unity;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.ViewModels;
using TwitchLeecher.Gui.Views;
using TwitchLeecher.Services.Interfaces;

namespace TwitchLeecher.Gui.Services
{
    public class GuiService : IGuiService
    {
        #region Fields

        private IUnityContainer container;

        private bool busy;

        #endregion Fields

        #region Constructor

        public GuiService(IUnityContainer container)
        {
            this.container = container;
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

        public MessageBoxResult ShowMessageBox(Window owner, string message)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message);
            msg.Owner = owner;
            msg.ShowDialog();

            return msg.Result;
        }

        public MessageBoxResult ShowMessageBox(Window owner, string message, string caption)
        {
            MessageBoxWindow msg = new MessageBoxWindow(message, caption);
            msg.Owner = owner;
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

            ILogService logService = this.container.Resolve<ILogService>();
            string logFile = logService.LogException(ex);

            MessageBoxWindow msg = new MessageBoxWindow("An unexpected error occured:"
                + Environment.NewLine + Environment.NewLine + ex.Message
                + Environment.NewLine + Environment.NewLine + "All details were written to log file"
                + Environment.NewLine + Environment.NewLine + logFile,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            msg.ShowDialog();
        }

        public void ShowSearchDialog(SearchParameters lastSearchParams, Action<bool, SearchParameters> dialogCompleteCallback)
        {
            if (lastSearchParams == null)
            {
                throw new ArgumentNullException(nameof(lastSearchParams));
            }

            if (dialogCompleteCallback == null)
            {
                throw new ArgumentNullException(nameof(dialogCompleteCallback));
            }

            SearchVM vm = this.container.Resolve<SearchVM>();
            vm.Username = lastSearchParams.Username;
            vm.VideoType = lastSearchParams.VideoType;
            vm.LoadLimit = lastSearchParams.LoadLimit;

            SearchWindow window = this.container.Resolve<SearchWindow>();
            window.DataContext = vm;

            bool? result = window.ShowDialog();

            SearchParameters resultObject = vm.ResultObject;

            dialogCompleteCallback(result != true, resultObject);
        }

        public void ShowDownloadDialog(TwitchVideo video, Action<bool, DownloadParameters> dialogCompleteCallback)
        {
            if (video == null)
            {
                throw new ArgumentNullException(nameof(video));
            }

            if (dialogCompleteCallback == null)
            {
                throw new ArgumentNullException(nameof(dialogCompleteCallback));
            }

            DownloadVM vm = this.container.Resolve<DownloadVM>();
            vm.Video = video;

            DownloadWindow window = this.container.Resolve<DownloadWindow>();
            window.DataContext = vm;

            bool? result = window.ShowDialog();

            DownloadParameters resultObject = vm.ResultObject;

            dialogCompleteCallback(result != true, resultObject);
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

        public void ShowLog(TwitchVideoDownload download)
        {
            if (download == null)
            {
                throw new ArgumentNullException(nameof(download));
            }

            LogVM vm = this.container.Resolve<LogVM>();
            vm.Download = download;

            LogWindow window = this.container.Resolve<LogWindow>();
            window.DataContext = vm;

            window.ShowDialog();
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