using Prism.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Services;
using TwitchLeecher.Services.Interfaces;

namespace TwitchLeecher.Gui.ViewModels
{
    public class DownloadsVM : BaseVM
    {
        #region Fields

        private ITwitchService twitchService;
        private IGuiService guiService;

        private ICommand retryDownloadCommand;
        private ICommand cancelDownloadCommand;
        private ICommand removeDownloadCommand;
        private ICommand viewCommand;
        private ICommand showLogCommand;

        private object commandLockObject;

        #endregion Fields

        #region Constructors

        public DownloadsVM(ITwitchService twitchService, IGuiService guiService)
        {
            this.twitchService = twitchService;
            this.guiService = guiService;

            this.twitchService.PropertyChanged += TwitchService_PropertyChanged;

            this.commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<TwitchVideoDownload> Downloads
        {
            get
            {
                return this.twitchService.Downloads;
            }
        }

        public ICommand RetryDownloadCommand
        {
            get
            {
                if (this.retryDownloadCommand == null)
                {
                    this.retryDownloadCommand = new DelegateCommand<string>(this.RetryDownload);
                }

                return this.retryDownloadCommand;
            }
        }

        public ICommand CancelDownloadCommand
        {
            get
            {
                if (this.cancelDownloadCommand == null)
                {
                    this.cancelDownloadCommand = new DelegateCommand<string>(this.CancelDownload);
                }

                return this.cancelDownloadCommand;
            }
        }

        public ICommand RemoveDownloadCommand
        {
            get
            {
                if (this.removeDownloadCommand == null)
                {
                    this.removeDownloadCommand = new DelegateCommand<string>(this.RemoveDownload);
                }

                return this.removeDownloadCommand;
            }
        }

        public ICommand ViewCommand
        {
            get
            {
                if (this.viewCommand == null)
                {
                    this.viewCommand = new DelegateCommand<string>(this.ViewVideo);
                }

                return this.viewCommand;
            }
        }

        public ICommand ShowLogCommand
        {
            get
            {
                if (this.showLogCommand == null)
                {
                    this.showLogCommand = new DelegateCommand<string>(this.ShowLog);
                }

                return this.showLogCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void RetryDownload(string id)
        {
            lock (this.commandLockObject)
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    this.twitchService.Retry(id);
                }
            }
        }

        private void CancelDownload(string id)
        {
            lock (this.commandLockObject)
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    this.twitchService.Cancel(id);
                }
            }
        }

        private void RemoveDownload(string id)
        {
            lock (this.commandLockObject)
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    this.twitchService.Remove(id);
                }
            }
        }

        private void ViewVideo(string id)
        {
            lock (this.commandLockObject)
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    TwitchVideoDownload download = this.Downloads.Where(d => d.Video.Id == id).FirstOrDefault();

                    if (download != null)
                    {
                        string folder = Path.GetDirectoryName(download.DownloadParams.Filename);

                        if (Directory.Exists(folder))
                        {
                            Process.Start(folder);
                        }
                    }
                }
            }
        }

        private void ShowLog(string id)
        {
            lock (this.commandLockObject)
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    TwitchVideoDownload download = this.Downloads.Where(d => d.Video.Id == id).FirstOrDefault();

                    if (download != null)
                    {
                        this.guiService.ShowLog(download);
                    }
                }
            }
        }

        #endregion Methods

        #region EventHandlers

        private void TwitchService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
        }

        #endregion EventHandlers
    }
}