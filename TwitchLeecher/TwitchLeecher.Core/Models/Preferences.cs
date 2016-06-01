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

        public const string DEFAULT_LOAD_LIMIT = "10";

        #endregion Constants

        #region Static Fields

        private static List<string> LOAD_LIMITS;

        #endregion Static Fields

        #region Fields

        private bool appCheckForUpdates;

        private string searchChannelName;

        private VideoType searchVideoType;

        private string searchLoadLimit;

        private bool searchOnStartup;

        private string downloadTempFolder;

        private string downloadFolder;

        private string downloadFileName;

        private VideoQuality downloadVideoQuality;

        private bool downloadRemoveCompleted;

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

        public string SearchLoadLimit
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

        public string DownloadTempFolder
        {
            get
            {
                return this.downloadTempFolder;
            }
            set
            {
                this.SetProperty(ref this.downloadTempFolder, value);
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

        public bool DownloadRemoveCompleted
        {
            get
            {
                return this.downloadRemoveCompleted;
            }
            set
            {
                this.SetProperty(ref this.downloadRemoveCompleted, value);
            }
        }

        #endregion Properties

        #region Static Methods

        public static List<string> GetLoadLimits()
        {
            if (LOAD_LIMITS == null)
            {
                LOAD_LIMITS = new List<string>();
                LOAD_LIMITS.Add("10");
                LOAD_LIMITS.Add("25");
                LOAD_LIMITS.Add("50");
                LOAD_LIMITS.Add("100");
                LOAD_LIMITS.Add("All");
            }

            return LOAD_LIMITS;
        }

        #endregion Static Methods

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
                if (!LOAD_LIMITS.Contains(this.searchLoadLimit))
                {
                    this.AddError(currentProperty, "Load limit has to be in '" + string.Join(", ", LOAD_LIMITS) + "'!");
                }
            }

            currentProperty = nameof(this.DownloadTempFolder);

            if (string.IsNullOrWhiteSpace(propertyName) || propertyName == currentProperty)
            {
                if (string.IsNullOrWhiteSpace(this.DownloadTempFolder))
                {
                    this.AddError(currentProperty, "Please specify a temporary download folder!");
                }
                else if (!FileSystem.HasWritePermission(this.downloadTempFolder))
                {
                    this.AddError(currentProperty, "You do not have write permissions on the specified folder! Please choose a different one!");
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
                DownloadTempFolder = this.DownloadTempFolder,
                DownloadFolder = this.DownloadFolder,
                DownloadFileName = this.DownloadFileName,
                DownloadVideoQuality = this.DownloadVideoQuality,
                DownloadRemoveCompleted = this.DownloadRemoveCompleted
            };

            return clone;
        }

        #endregion Methods
    }
}