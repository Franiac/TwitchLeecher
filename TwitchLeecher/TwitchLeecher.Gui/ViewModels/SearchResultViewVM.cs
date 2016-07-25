using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.ViewModels
{
    public class SearchResultViewVM : ViewModelBase
    {
        #region Fields

        private ITwitchService twitchService;
        private IDialogService dialogService;
        private INavigationService navigationService;
        private INotificationService notificationService;
        private IEventAggregator eventAggregator;
        private IPreferencesService preferencesService;
        private IFilenameService filenameService;

        private ICommand viewCommand;
        private ICommand downloadCommand;
        private ICommand seachCommand;

        private object commandLockObject;

        #endregion Fields

        #region Constructors

        public SearchResultViewVM(
            ITwitchService twitchService,
            IDialogService dialogService,
            INavigationService navigationService,
            INotificationService notificationService,
            IEventAggregator eventAggregator,
            IPreferencesService preferencesService,
            IFilenameService filenameService)
        {
            this.twitchService = twitchService;
            this.dialogService = dialogService;
            this.navigationService = navigationService;
            this.notificationService = notificationService;
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

        public ICommand SeachCommnad
        {
            get
            {
                if (this.seachCommand == null)
                {
                    this.seachCommand = new DelegateCommand(this.ShowSearch);
                }

                return this.seachCommand;
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
                this.dialogService.ShowAndLogException(ex);
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

                            DownloadParameters downloadParams = new DownloadParameters(video, resolution, currentPrefs.DownloadFolder, filename);

                            this.navigationService.ShowDownload(downloadParams);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        public void ShowSearch()
        {
            try
            {
                lock (this.commandLockObject)
                {
                    this.navigationService.ShowSearch();
                }
            }
            catch (Exception ex)
            {
                this.dialogService.ShowAndLogException(ex);
            }
        }

        protected override List<MenuCommand> BuildMenu()
        {
            List<MenuCommand> menuCommands = base.BuildMenu();

            if (menuCommands == null)
            {
                menuCommands = new List<MenuCommand>();
            }

            menuCommands.Add(new MenuCommand(this.SeachCommnad, "New Search", "Search"));

            return menuCommands;
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