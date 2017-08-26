using System;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class Preferences : BindableBase
    {
        #region Fields

        private Version _version;

        private bool _appCheckForUpdates;

        private bool _appShowDonationButton;

        private string _searchChannelName;

        private VideoType _searchVideoType;

        private bool _searchLoadOnlyToday;

        private int _searchLoadLastDays;

        private bool _searchOnStartup;

        private string _downloadTempFolder;

        private string _downloadFolder;

        private string _downloadFileName;

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

        public bool SearchLoadOnlyToday
        {
            get
            {
                return _searchLoadOnlyToday;
            }
            set
            {
                SetProperty(ref _searchLoadOnlyToday, value);
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

            currentProperty = nameof(SearchLoadLastDays);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (!_searchLoadOnlyToday && (_searchLoadLastDays < 1 || _searchLoadLastDays > 99))
                {
                    AddError(currentProperty, "Value has to be between 1 and 99!");
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
                SearchLoadOnlyToday = SearchLoadOnlyToday,
                SearchLoadLastDays = SearchLoadLastDays,
                SearchOnStartup = SearchOnStartup,
                DownloadTempFolder = DownloadTempFolder,
                DownloadFolder = DownloadFolder,
                DownloadFileName = DownloadFileName,
                DownloadRemoveCompleted = DownloadRemoveCompleted
            };

            return clone;
        }

        #endregion Methods
    }
}