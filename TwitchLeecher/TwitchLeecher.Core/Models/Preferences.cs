﻿using System;
using System.Collections.Generic;
using System.IO;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Shared.Helpers;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class Preferences : BindableBase
    {
        #region Static Fields

        public static List<DefaultQuality> DefaultQualities = new List<DefaultQuality>() {
            new DefaultQuality(DefaultQuality.SOURCE_RES, "Source"),
            new DefaultQuality(720, "720p"),
            new DefaultQuality(480, "480p"),
            new DefaultQuality(360, "360p"),
            new DefaultQuality(160, "160p"),
            new DefaultQuality(DefaultQuality.AUDIO_ONLY_RES, "Audio Only")
        };

        #endregion Static Fields

        #region Fields

        private Version _version;

        private bool _appCheckForUpdates;

        private bool _appShowDonationButton;

        private RangeObservableCollection<string> _searchFavouriteChannels;

        private string _searchChannelName;

        private VideoType _searchVideoType;

        private LoadLimitType _searchLoadLimitType;

        private int _searchLoadLastDays;

        private int _searchLoadLastVods;

        private bool _searchOnStartup;

        private string _downloadTempFolder;

        private string _downloadFolder;

        private string _downloadFileName;

        private DefaultQuality _downloadDefaultQuality;

        private bool _downloadSubfoldersForFav;

        private bool _downloadRemoveCompleted;

        private bool _downloadDisableConversion;

        private bool _miscUseExternalPlayer;

        private string _miscExternalPlayer;

        #endregion Fields

        #region Properties

        public Version Version
        {
            get
            {
                return _version;
            }
            set
            {
                SetProperty(ref _version, value);
            }
        }

        public bool AppCheckForUpdates
        {
            get
            {
                return _appCheckForUpdates;
            }
            set
            {
                SetProperty(ref _appCheckForUpdates, value);
            }
        }

        public bool AppShowDonationButton
        {
            get
            {
                return _appShowDonationButton;
            }
            set
            {
                SetProperty(ref _appShowDonationButton, value);
            }
        }

        public bool MiscUseExternalPlayer
        {
            get
            {
                return _miscUseExternalPlayer;
            }
            set
            {
                SetProperty(ref _miscUseExternalPlayer, value);
            }
        }

        public string MiscExternalPlayer
        {
            get
            {
                return _miscExternalPlayer;
            }

            set
            {
                SetProperty(ref _miscExternalPlayer, value);
            }
        }

        public RangeObservableCollection<string> SearchFavouriteChannels
        {
            get
            {
                if (_searchFavouriteChannels == null)
                {
                    _searchFavouriteChannels = new RangeObservableCollection<string>();
                }

                return _searchFavouriteChannels;
            }
        }

        public string SearchChannelName
        {
            get
            {
                return _searchChannelName;
            }
            set
            {
                SetProperty(ref _searchChannelName, value);
            }
        }

        public VideoType SearchVideoType
        {
            get
            {
                return _searchVideoType;
            }
            set
            {
                SetProperty(ref _searchVideoType, value);
            }
        }

        public LoadLimitType SearchLoadLimitType
        {
            get
            {
                return _searchLoadLimitType;
            }
            set
            {
                SetProperty(ref _searchLoadLimitType, value);
            }
        }

        public int SearchLoadLastDays
        {
            get
            {
                return _searchLoadLastDays;
            }
            set
            {
                SetProperty(ref _searchLoadLastDays, value);
            }
        }

        public int SearchLoadLastVods
        {
            get
            {
                return _searchLoadLastVods;
            }
            set
            {
                SetProperty(ref _searchLoadLastVods, value);
            }
        }

        public bool SearchOnStartup
        {
            get
            {
                return _searchOnStartup;
            }
            set
            {
                SetProperty(ref _searchOnStartup, value);
            }
        }

        public string DownloadTempFolder
        {
            get
            {
                return _downloadTempFolder;
            }
            set
            {
                SetProperty(ref _downloadTempFolder, value);
            }
        }

        public string DownloadFolder
        {
            get
            {
                return _downloadFolder;
            }
            set
            {
                SetProperty(ref _downloadFolder, value);
            }
        }

        public string DownloadFileName
        {
            get
            {
                return _downloadFileName;
            }
            set
            {
                SetProperty(ref _downloadFileName, value);
            }
        }

        public DefaultQuality DownloadDefaultQuality
        {
            get
            {
                return _downloadDefaultQuality;
            }
            set
            {
                SetProperty(ref _downloadDefaultQuality, value);
            }
        }

        public List<DefaultQuality> DownloadDefaultQualities
        {
            get
            {
                return DefaultQualities;
            }
        }

        public bool DownloadSubfoldersForFav
        {
            get
            {
                return _downloadSubfoldersForFav;
            }
            set
            {
                SetProperty(ref _downloadSubfoldersForFav, value);
            }
        }

        public bool DownloadRemoveCompleted
        {
            get
            {
                return _downloadRemoveCompleted;
            }
            set
            {
                SetProperty(ref _downloadRemoveCompleted, value);
            }
        }

        public bool DownloadDisableConversion
        {
            get
            {
                return _downloadDisableConversion;
            }
            set
            {
                SetProperty(ref _downloadDisableConversion, value);
            }
        }

        #endregion Properties

        #region Methods

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(MiscExternalPlayer);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (MiscUseExternalPlayer)
                {
                    if (string.IsNullOrWhiteSpace(_miscExternalPlayer))
                    {
                        AddError(currentProperty, "Please specify an external player!");
                    }
                    else if (!_miscExternalPlayer.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        AddError(currentProperty, "Filename must be an executable!");
                    }
                    else if (!File.Exists(_miscExternalPlayer))
                    {
                        AddError(currentProperty, "The specified file does not exist!");
                    }
                }
            }

            currentProperty = nameof(SearchChannelName);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_searchOnStartup && string.IsNullOrWhiteSpace(_searchChannelName))
                {
                    AddError(currentProperty, "If 'Search on Startup' is enabled, you need to specify a default channel name!");
                }
            }

            currentProperty = nameof(SearchLoadLastDays);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_searchLoadLimitType == LoadLimitType.Timespan && (_searchLoadLastDays < 1 || _searchLoadLastDays > 999))
                {
                    AddError(currentProperty, "Value has to be between 1 and 999!");
                }
            }

            currentProperty = nameof(SearchLoadLastVods);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_searchLoadLimitType == LoadLimitType.LastVods && (_searchLoadLastVods < 1 || _searchLoadLastVods > 999))
                {
                    AddError(currentProperty, "Value has to be between 1 and 999!");
                }
            }

            currentProperty = nameof(DownloadTempFolder);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(DownloadTempFolder))
                {
                    AddError(currentProperty, "Please specify a temporary download folder!");
                }

                // Assume max length of .ts filename in playlist is '99999999.ts' => 11 characters
                if (DownloadTempFolder.Length + 11 > 250)
                {
                    AddError(currentProperty, "The length of this folder path must be shorter than 239 characters!");
                }

                if (!Directory.Exists(DownloadTempFolder))
                {
                    AddError(currentProperty, "The selected Temporary Folder does not exist!");
                }
            }

            currentProperty = nameof(DownloadFolder);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(_downloadFolder))
                {
                    AddError(currentProperty, "Please specify a default download folder!");
                }

                if (!Directory.Exists(_downloadFolder))
                {
                    AddError(currentProperty, "The selected Download Folder does not exist!");
                }
            }

            currentProperty = nameof(DownloadFileName);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(_downloadFileName))
                {
                    AddError(currentProperty, "Please specify a default download filename!");
                }
                else if (_downloadFileName.Contains(".") || FileSystem.FilenameContainsInvalidChars(_downloadFileName))
                {
                    string invalidChars = new string(Path.GetInvalidFileNameChars());

                    AddError(currentProperty, $"Filename contains invalid characters ({invalidChars}.)!");
                }
            }
        }

        public Preferences Clone()
        {
            Preferences clone = new Preferences()
            {
                Version = Version,
                AppCheckForUpdates = AppCheckForUpdates,
                AppShowDonationButton = AppShowDonationButton,
                MiscUseExternalPlayer = MiscUseExternalPlayer,
                MiscExternalPlayer = MiscExternalPlayer,
                SearchChannelName = SearchChannelName,
                SearchVideoType = SearchVideoType,
                SearchLoadLimitType = SearchLoadLimitType,
                SearchLoadLastDays = SearchLoadLastDays,
                SearchLoadLastVods = SearchLoadLastVods,
                SearchOnStartup = SearchOnStartup,
                DownloadTempFolder = DownloadTempFolder,
                DownloadFolder = DownloadFolder,
                DownloadFileName = DownloadFileName,
                DownloadDefaultQuality = _downloadDefaultQuality,
                DownloadSubfoldersForFav = DownloadSubfoldersForFav,
                DownloadRemoveCompleted = DownloadRemoveCompleted,
                DownloadDisableConversion = DownloadDisableConversion
            };

            clone.SearchFavouriteChannels.AddRange(SearchFavouriteChannels);

            return clone;
        }

        #endregion Methods
    }
}