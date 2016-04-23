using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Services;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.ViewModels
{
    public class VideosViewVM : ViewModelBase
    {
        #region Fields

        private ITwitchService twitchService;
        private IGuiService guiService;
        private IEventAggregator eventAggregator;
        private IPreferencesService preferencesService;
        private IFilenameService filenameService;

        private ICommand viewCommand;
        private ICommand downloadCommand;

        private object commandLockObject;

        #endregion Fields

        #region Constructors

        public VideosViewVM(ITwitchService twitchService,
            IGuiService guiService,
            IEventAggregator eventAggregator,
            IPreferencesService preferencesService,
            IFilenameService filenameService)
        {
            this.twitchService = twitchService;
            this.guiService = guiService;
            this.eventAggregator = eventAggregator;
            this.preferencesService = preferencesService;
            this.filenameService = filenameService;

            this.twitchService.PropertyChanged += TwitchService_PropertyChanged;

            this.commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<TwitchVideo> Videos
        {
            get
            {
                return this.twitchService.Videos;
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

        public ICommand DownloadCommand
        {
            get
            {
                if (this.downloadCommand == null)
                {
                    this.downloadCommand = new DelegateCommand<string>(this.DownloadVideo);
                }

                return this.downloadCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void ViewVideo(string id)
        {
            try
            {
                lock (this.commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        TwitchVideo video = this.Videos.Where(v => v.Id == id).FirstOrDefault();

                        if (video != null && video.Url != null && video.Url.IsAbsoluteUri)
                        {
                            Process.Start(video.Url.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        private void DownloadVideo(string id)
        {
            try
            {
                lock (this.commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        if (this.twitchService.Downloads.Where(d => d.Video.Id == id).Any())
                        {
                            this.guiService.ShowMessageBox("This video is already being downloaded!", "Download Video", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        TwitchVideo video = this.Videos.Where(v => v.Id == id).FirstOrDefault();

                        if (video != null)
                        {
                            Preferences currentPrefs = this.preferencesService.CurrentPreferences.Clone();

                            TwitchVideoResolution resolution = video.Resolutions.Where(r => r.VideoQuality == currentPrefs.DownloadVideoQuality).FirstOrDefault();

                            if (resolution == null)
                            {
                                resolution = video.Resolutions.First();
                            }

                            string filename = this.filenameService.SubstituteWildcards(currentPrefs.DownloadFileName, video);

                            this.guiService.ShowDownloadDialog(video, resolution, currentPrefs.DownloadFolder, filename, this.DownloadCallback);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        public void DownloadCallback(bool cancelled, DownloadParameters downloadParams)
        {
            try
            {
                if (!cancelled)
                {
                    this.twitchService.Enqueue(downloadParams);
                    this.eventAggregator.GetEvent<ShowDownloadsEvent>().Publish();
                }
            }
            catch (Exception ex)
            {
                this.guiService.ShowAndLogException(ex);
            }
        }

        #endregion Methods

        #region EventHandlers

        private void TwitchService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.FirePropertyChanged(e.PropertyName);
        }

        #endregion EventHandlers
    }
}