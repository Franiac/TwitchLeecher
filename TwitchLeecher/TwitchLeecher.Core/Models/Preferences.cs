using System;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Core.Models
{
    public class Preferences : BindableBase
    {
        #region Fields

        private bool appCheckForUpdates;

        private string searchChannelName;

        private VideoType searchVideoType;

        private int searchLoadLimit;

        private bool searchOnStartup;

        private string downloadFolder;

        private string downloadFileName;

        private VideoQuality downloadVideoQuality;

        #endregion Fields

        #region Properties

        public bool AppCheckForUpdates
        {
            get
            {
                return this.appCheckForUpdates;
            }
            set
            {
                this.SetProperty(ref this.appCheckForUpdates, value);
            }
        }

        public string SearchChannelName
        {
            get
            {
                return this.searchChannelName;
            }
            set
            {
                this.SetProperty(ref this.searchChannelName, value);
            }
        }

        public VideoType SearchVideoType
        {
            get
            {
                return this.searchVideoType;
            }
            set
            {
                this.SetProperty(ref this.searchVideoType, value);
            }
        }

        public int SearchLoadLimit
        {
            get
            {
                return this.searchLoadLimit;
            }
            set
            {
                this.SetProperty(ref this.searchLoadLimit, value);
            }
        }

        public bool SearchOnStartup
        {
            get
            {
                return this.searchOnStartup;
            }
            set
            {
                this.SetProperty(ref this.searchOnStartup, value);
            }
        }

        public string DownloadFolder
        {
            get
            {
                return this.downloadFolder;
            }
            set
            {
                this.SetProperty(ref this.downloadFolder, value);
            }
        }

        public string DownloadFileName
        {
            get
            {
                return this.downloadFileName;
            }
            set
            {
                this.SetProperty(ref this.downloadFileName, value);
            }
        }

        public VideoQuality DownloadVideoQuality
        {
            get
            {
                return this.downloadVideoQuality;
            }
            set
            {
                this.SetProperty(ref this.downloadVideoQuality, value);
            }
        }

        #endregion Properties

        #region Methods

        public override void Validate(string propertyName = null)
        {
            base.Validate(propertyName);

            string currentProperty = nameof(this.SearchChannelName);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (this.searchOnStartup && string.IsNullOrWhiteSpace(this.searchChannelName))
                {
                    this.AddError(currentProperty, "If 'Search on Startup' is enabled, you need to specify a default channel name!");
                }
            }

            currentProperty = nameof(this.SearchLoadLimit);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (this.searchLoadLimit < 1 || this.searchLoadLimit > 100)
                {
                    this.AddError(currentProperty, "Load limit has to be a value between 1 and 100!");
                }
            }

            currentProperty = nameof(this.DownloadFolder);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(this.downloadFolder))
                {
                    this.AddError(currentProperty, "Please specify a default download folder!");
                }
                else if (!FileSystem.HasWritePermission(this.downloadFolder))
                {
                    this.AddError(currentProperty, "You do not have write permissions on the specified folder! Please choose a different one!");
                }
            }

            currentProperty = nameof(this.DownloadFileName);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(this.downloadFileName))
                {
                    this.AddError(currentProperty, "Please specify a default download filename!");
                }
                else if (!this.downloadFileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    this.AddError(currentProperty, "Filename must end with '.mp4'!");
                }
                else if (FileSystem.FilenameContainsInvalidChars(this.downloadFileName))
                {
                    this.AddError(currentProperty, "Filename contains invalid characters!");
                }
            }
        }

        public Preferences Clone()
        {
            Preferences clone = new Preferences()
            {
                AppCheckForUpdates = this.AppCheckForUpdates,
                SearchChannelName = this.SearchChannelName,
                SearchVideoType = this.SearchVideoType,
                SearchLoadLimit = this.SearchLoadLimit,
                SearchOnStartup = this.SearchOnStartup,
                DownloadFolder = this.DownloadFolder,
                DownloadFileName = this.DownloadFileName,
                DownloadVideoQuality = this.DownloadVideoQuality
            };

            return clone;
        }

        #endregion Methods
    }
}