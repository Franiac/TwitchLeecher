using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Commands;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Gui.ViewModels
{
    public class SearchResultViewVM : ViewModelBase, INavigationState
    {
        #region Fields

        private readonly ITwitchService _twitchService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationsService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IPreferencesService _preferencesService;
        private readonly IFilenameService _filenameService;

        private readonly object _commandLockObject;

        private ICommand _viewCommand;
        private ICommand _downloadCommand;
        private ICommand _seachCommand;

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
            _twitchService = twitchService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _notificationsService = notificationService;
            _eventAggregator = eventAggregator;
            _preferencesService = preferencesService;
            _filenameService = filenameService;

            _twitchService.PropertyChanged += TwitchService_PropertyChanged;

            _commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public double ScrollPosition { get; set; }

        public ObservableCollection<TwitchVideo> Videos
        {
            get
            {
                return _twitchService.Videos;
            }
        }

        public ICommand ViewCommand
        {
            get
            {
                if (_viewCommand == null)
                {
                    _viewCommand = new DelegateCommand<string>(ViewVideo);
                }

                return _viewCommand;
            }
        }

        public ICommand DownloadCommand
        {
            get
            {
                if (_downloadCommand == null)
                {
                    _downloadCommand = new DelegateCommand<string>(DownloadVideo);
                }

                return _downloadCommand;
            }
        }

        public ICommand SeachCommnad
        {
            get
            {
                if (_seachCommand == null)
                {
                    _seachCommand = new DelegateCommand(ShowSearch);
                }

                return _seachCommand;
            }
        }

        #endregion Properties

        #region Methods

        private void ViewVideo(string id)
        {
            try
            {
                lock (_commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        TwitchVideo video = Videos.Where(v => v.Id == id).FirstOrDefault();

                        if (video != null && video.Url != null && video.Url.IsAbsoluteUri)
                        {
                            StartVideoStream(video);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void StartVideoStream(TwitchVideo video)
        {
            try
            {
                Preferences currentPrefs = _preferencesService.CurrentPreferences;

                if (currentPrefs.MiscUseExternalPlayer)
                {
                    Process.Start(currentPrefs.MiscExternalPlayer, video.Url.ToString());
                }
                else
                {
                    Process.Start(video.Url.ToString());
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private void DownloadVideo(string id)
        {
            try
            {
                lock (_commandLockObject)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        TwitchVideo video = Videos.Where(v => v.Id == id).FirstOrDefault();

                        if (video != null)
                        {
                            VodAuthInfo vodAuthInfo = _twitchService.RetrieveVodAuthInfo(video.Id);

                            if (!vodAuthInfo.Privileged && vodAuthInfo.SubOnly)
                            {
                                if (!_twitchService.IsAuthorized)
                                {
                                    _dialogService.ShowMessageBox("This video is sub-only! Please authorize your Twitch account by clicking the Twitch button in the menu.", "SUB HYPE!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                }
                                else
                                {
                                    _dialogService.ShowMessageBox("This video is sub-only but you are not subscribed to '" + video.Channel + "'!", "SUB HYPE!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                }

                                return;
                            }

                            Preferences currentPrefs = _preferencesService.CurrentPreferences.Clone();

                            string folder = currentPrefs.DownloadSubfoldersForFav && _preferencesService.IsChannelInFavourites(video.Channel)
                                ? Path.Combine(currentPrefs.DownloadFolder, video.Channel)
                                : currentPrefs.DownloadFolder;

                            if (video.Length < currentPrefs.DownloadSplitTime)
                                currentPrefs.DownloadSplitUse = false;

                            string fileName = currentPrefs.DownloadFileName;
                            fileName = _filenameService.EnsureExtension(fileName, currentPrefs.DownloadDisableConversion);
                            if (currentPrefs.DownloadSplitUse == true)
                            {
                                fileName = fileName.Replace(FilenameWildcards.UNIQNUMBER, FilenameWildcards.UNIQNUMBER.Insert(FilenameWildcards.UNIQNUMBER.Length - 1, "_TEMP"));
                                fileName = _filenameService.SubstituteWildcards(fileName, folder, _twitchService.IsFileNameUsed, video);
                                fileName = fileName.Replace(FilenameWildcards.UNIQNUMBER.Insert(FilenameWildcards.UNIQNUMBER.Length - 1, "_TEMP"), FilenameWildcards.UNIQNUMBER);
                            }
                            else
                                fileName = _filenameService.SubstituteWildcards(fileName, folder, _twitchService.IsFileNameUsed, video);

                            TwitchVideoQuality bestQual = SolveQuality(video.Qualities, currentPrefs.DownloadQuality);

                            DownloadParameters downloadParams = new DownloadParameters(video, vodAuthInfo, bestQual, folder, fileName, currentPrefs.DownloadSplitTime, currentPrefs.DownloadDisableConversion);

                            _navigationService.ShowDownload(downloadParams);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        private TwitchVideoQuality SolveQuality(List<TwitchVideoQuality> qualities, Core.Enums.VideoQuality shouldQuality)
        {
            if (qualities.Count == 1 || shouldQuality == Core.Enums.VideoQuality.Source)
                return qualities.First();
            if (shouldQuality == Core.Enums.VideoQuality.AudioOnly)
            {
                return qualities.Find(x => x.DisplayString == "Audio Only") ?? qualities.First();
            }
            int fpsShould = int.Parse(shouldQuality.ToString().Substring(shouldQuality.ToString().IndexOf("f") + 1));
            int resolutionShould = int.Parse(shouldQuality.ToString().Substring(1, shouldQuality.ToString().IndexOf("f") - 1));

            TwitchVideoQuality bestResult = qualities.Find(x => x.Fps.HasValue && x.ResolutionY.HasValue);
            if (bestResult == null)
                return qualities.First();
            foreach (TwitchVideoQuality qual in qualities)
            {
                if (!qual.ResolutionY.HasValue) continue;
                if (Math.Abs(qual.ResolutionY.Value - resolutionShould) < Math.Abs(bestResult.ResolutionY.Value - resolutionShould))
                {
                    bestResult = qual;
                }
                else if (Math.Abs(qual.ResolutionY.Value - resolutionShould) == Math.Abs(bestResult.ResolutionY.Value - resolutionShould)
                    && Math.Abs(qual.Fps.Value - fpsShould) == Math.Abs(bestResult.Fps.Value - fpsShould))
                {
                    bestResult = qual;
                }
            }

            return bestResult;
        }

        public void ShowSearch()
        {
            try
            {
                lock (_commandLockObject)
                {
                    _navigationService.ShowSearch();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowAndLogException(ex);
            }
        }

        protected override List<MenuCommand> BuildMenu()
        {
            List<MenuCommand> menuCommands = base.BuildMenu();

            if (menuCommands == null)
            {
                menuCommands = new List<MenuCommand>();
            }

            menuCommands.Add(new MenuCommand(SeachCommnad, "New Search", "Search"));

            return menuCommands;
        }

        #endregion Methods

        #region EventHandlers

        private void TwitchService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;

            FirePropertyChanged(propertyName);

            if (propertyName.Equals(nameof(Videos)))
            {
                ScrollPosition = 0;
            }
        }

        #endregion EventHandlers
    }
}