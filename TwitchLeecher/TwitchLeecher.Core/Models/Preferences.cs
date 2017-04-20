using System;
using System.Collections.Generic;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class Preferences : BindableBase
    {
        #region Constants

        public const int DEFAULT_LOAD_LIMIT = 10;

        #endregion Constants

        #region Static Fields

        private static List<int> LOAD_LIMITS;

        #endregion Static Fields

        #region Fields

        private Version _version;

        private bool _appCheckForUpdates;

        private bool _appShowDonationButton;

        private string _searchChannelName;

        private VideoType _searchVideoType;

        private int _searchLoadLimit;

        private bool _searchOnStartup;

        private string _downloadTempFolder;

        private string _downloadFolder;

        private string _downloadFileName;

        private string _downloadVideoQuality;

        private bool _downloadRemoveCompleted;

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

        public int SearchLoadLimit
        {
            get
            {
                return _searchLoadLimit;
            }
            set
            {
                SetProperty(ref _searchLoadLimit, value);
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

        public string DownloadVideoQuality
        {
            get
            {
                return _downloadVideoQuality;
            }
            set
            {
                SetProperty(ref _downloadVideoQuality, value);
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

        #endregion Properties

        #region Static Methods

        public static List<int> GetLoadLimits()
        {
            if (LOAD_LIMITS == null)
            {
                LOAD_LIMITS = new List<int>
                {
                    10,
                    25,
                    50,
                    100,
                    250,
                    500,
                    1000
                };
            }

            return LOAD_LIMITS;
        }

        #endregion Static Methods

        #region Methods

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(SearchChannelName);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (_searchOnStartup && string.IsNullOrWhiteSpace(_searchChannelName))
                {
                    AddError(currentProperty, "If 'Search on Startup' is enabled, you need to specify a default channel name!");
                }
            }

            currentProperty = nameof(SearchLoadLimit);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                List<int> loadLimits = GetLoadLimits();

                if (!loadLimits.Contains(_searchLoadLimit))
                {
                    AddError(currentProperty, "Load limit has to be in '" + string.Join(", ", loadLimits) + "'!");
                }
            }

            currentProperty = nameof(DownloadTempFolder);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(DownloadTempFolder))
                {
                    AddError(currentProperty, "Please specify a temporary download folder!");
                }
                else if (!FileSystem.HasWritePermission(_downloadTempFolder))
                {
                    AddError(currentProperty, "You do not have write permissions on the specified folder! Please choose a different one!");
                }
            }

            currentProperty = nameof(DownloadFolder);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(_downloadFolder))
                {
                    AddError(currentProperty, "Please specify a default download folder!");
                }
                else if (!FileSystem.HasWritePermission(_downloadFolder))
                {
                    AddError(currentProperty, "You do not have write permissions on the specified folder! Please choose a different one!");
                }
            }

            currentProperty = nameof(DownloadFileName);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(_downloadFileName))
                {
                    AddError(currentProperty, "Please specify a default download filename!");
                }
                else if (!_downloadFileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    AddError(currentProperty, "Filename must end with '.mp4'!");
                }
                else if (FileSystem.FilenameContainsInvalidChars(_downloadFileName))
                {
                    AddError(currentProperty, "Filename contains invalid characters!");
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
                SearchChannelName = SearchChannelName,
                SearchVideoType = SearchVideoType,
                SearchLoadLimit = SearchLoadLimit,
                SearchOnStartup = SearchOnStartup,
                DownloadTempFolder = DownloadTempFolder,
                DownloadFolder = DownloadFolder,
                DownloadFileName = DownloadFileName,
                DownloadVideoQuality = DownloadVideoQuality,
                DownloadRemoveCompleted = DownloadRemoveCompleted
            };

            return clone;
        }

        #endregion Methods
    }
}