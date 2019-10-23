using System;
using System.IO;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Shared.Helpers;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class Preferences : BindableBase
    {
        static public int MinSplitLength { get { return 120; } }//in seconds
        //At least 60 seconds

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

        private VideoQuality _downloadQuality;

        private bool _downloadSubfoldersForFav;

        private bool _downloadRemoveCompleted;

        private bool _downloadDisableConversion;
        
        private bool _downloadSplitUse;

        private TimeSpan _downloadSplitTime;

        private int _splitOverlapSeconds;

        private bool _downloadAndConcatSimultaneously;

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

        public VideoQuality DownloadQuality
        {
            get
            {
                return _downloadQuality;
            }
            set
            {
                SetProperty(ref _downloadQuality, value);
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

        public bool DownloadAndConcatSimultaneously
        {
            get
            {
                return _downloadAndConcatSimultaneously;
            }
            set
            {
                SetProperty(ref _downloadAndConcatSimultaneously, value);
            }
        }

        public bool DownloadSplitUse
        {
            get
            {
                return _downloadSplitUse;
            }
            set
            {
                SetProperty(ref _downloadSplitUse, value);
            }
        }

        public TimeSpan DownloadSplitTime
        {
            get
            {
                return _downloadSplitTime;
            }
            set
            {
                SetProperty(ref _downloadSplitTime, value);
            }
        }

        public int SplitOverlapSeconds
        {
            get
            {
                return _splitOverlapSeconds;
            }
            set
            {
                SetProperty(ref _splitOverlapSeconds, value);
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
            }

            currentProperty = nameof(DownloadFolder);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(_downloadFolder))
                {
                    AddError(currentProperty, "Please specify a default download folder!");
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

            currentProperty = nameof(DownloadSplitUse);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_downloadSplitUse && !_downloadDisableConversion && !_downloadFileName.Contains(FilenameWildcards.UNIQNUMBER))
                {
                    string errorMessage = $"With autosplit option enabled, download file name has to contain {FilenameWildcards.UNIQNUMBER} for autonaming!";
                    AddError(currentProperty, errorMessage);
                    AddError(nameof(DownloadFileName), errorMessage);
                }
            }

            currentProperty = nameof(DownloadSplitTime);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_downloadSplitUse && !_downloadDisableConversion && _downloadSplitTime.TotalSeconds < Preferences.MinSplitLength)
                {
                    string errorMessage = $"Split time has to be equal or more {Preferences.MinSplitLength} seconds!";
                    AddError(currentProperty, errorMessage);
                    AddError(nameof(DownloadSplitUse), errorMessage);
                }
            }

            currentProperty = nameof(SplitOverlapSeconds);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_downloadSplitUse && !_downloadDisableConversion && (_splitOverlapSeconds >= Preferences.MinSplitLength / 2 || _splitOverlapSeconds < 0))
                {
                    string errorMessage = $"Overlap seconds has to be less than {Preferences.MinSplitLength / 2} seconds!";
                    AddError(currentProperty, errorMessage);
                    AddError(nameof(DownloadSplitUse), errorMessage);
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
                DownloadQuality = DownloadQuality,
                DownloadSubfoldersForFav = DownloadSubfoldersForFav,
                DownloadRemoveCompleted = DownloadRemoveCompleted,
                DownloadDisableConversion = DownloadDisableConversion,
                DownloadAndConcatSimultaneously = DownloadAndConcatSimultaneously,
                DownloadSplitUse = DownloadSplitUse,
                DownloadSplitTime = DownloadSplitTime,
                SplitOverlapSeconds = SplitOverlapSeconds,
            };

            clone.SearchFavouriteChannels.AddRange(SearchFavouriteChannels);

            return clone;
        }
        #endregion Methods
    }
}