using System.IO;
using System.Xml.Linq;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Events;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.IO;

namespace TwitchLeecher.Services.Services
{
    internal class PreferencesService : IPreferencesService
    {
        #region Constants

        private const string CONFIG_FILE = "config.xml";

        private const string PREFERENCES_EL = "Preferences";

        private const string APP_EL = "Application";
        private const string APP_CHECKFORUPDATES_EL = "CheckForUpdates";

        private const string SEARCH_EL = "Search";
        private const string SEARCH_CHANNELNAME_EL = "ChannelName";
        private const string SEARCH_VIDEOTYPE_EL = "VideoType";
        private const string SEARCH_LOADLIMIT_EL = "LoadLimit";
        private const string SEARCH_SEARCHONSTARTUP_EL = "SearchOnStartup";

        private const string DOWNLOAD_EL = "Download";
        private const string DOWNLOAD_FOLDER_EL = "Folder";
        private const string DOWNLOAD_FILENAME_EL = "FileName";
        private const string DOWNLOAD_VIDEOQUALITY_EL = "VideoQuality";

        #endregion Constants

        #region Fields

        private IFolderService folderService;
        private IEventAggregator eventAggregator;

        private Preferences currentPreferences;

        #endregion Fields

        #region Constructors

        public PreferencesService(IFolderService folderService, IEventAggregator eventAggregator)
        {
            this.folderService = folderService;
            this.eventAggregator = eventAggregator;
        }

        #endregion Constructors

        #region Properties

        public Preferences CurrentPreferences
        {
            get
            {
                if (this.currentPreferences == null)
                {
                    this.currentPreferences = this.Load();
                }

                return this.currentPreferences;
            }
        }

        #endregion Properties

        #region Methods

        public void Save(Preferences preferences)
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", null));

            XElement preferencesEl = new XElement(PREFERENCES_EL);
            doc.Add(preferencesEl);

            XElement appEl = new XElement(APP_EL);
            preferencesEl.Add(appEl);

            XElement searchEl = new XElement(SEARCH_EL);
            preferencesEl.Add(searchEl);

            XElement downloadEl = new XElement(DOWNLOAD_EL);
            preferencesEl.Add(downloadEl);

            // Application
            XElement appCheckForUpdatesEl = new XElement(APP_CHECKFORUPDATES_EL);
            appCheckForUpdatesEl.SetValue(preferences.AppCheckForUpdates);
            appEl.Add(appCheckForUpdatesEl);

            // Search
            if (!string.IsNullOrWhiteSpace(preferences.SearchChannelName))
            {
                XElement searchChannelNameEl = new XElement(SEARCH_CHANNELNAME_EL);
                searchChannelNameEl.SetValue(preferences.SearchChannelName);
                searchEl.Add(searchChannelNameEl);
            }

            XElement searchVideoTypeEl = new XElement(SEARCH_VIDEOTYPE_EL);
            searchVideoTypeEl.SetValue(preferences.SearchVideoType);
            searchEl.Add(searchVideoTypeEl);

            XElement searchLoadLimitEl = new XElement(SEARCH_LOADLIMIT_EL);
            searchLoadLimitEl.SetValue(preferences.SearchLoadLimit);
            searchEl.Add(searchLoadLimitEl);

            XElement searchOnStartupEl = new XElement(SEARCH_SEARCHONSTARTUP_EL);
            searchOnStartupEl.SetValue(preferences.SearchOnStartup);
            searchEl.Add(searchOnStartupEl);

            // Download
            if (!string.IsNullOrWhiteSpace(preferences.DownloadFolder))
            {
                XElement downloadFolderEl = new XElement(DOWNLOAD_FOLDER_EL);
                downloadFolderEl.SetValue(preferences.DownloadFolder);
                downloadEl.Add(downloadFolderEl);
            }

            if (!string.IsNullOrWhiteSpace(preferences.DownloadFileName))
            {
                XElement downloadFileNameEl = new XElement(DOWNLOAD_FILENAME_EL);
                downloadFileNameEl.SetValue(preferences.DownloadFileName);
                downloadEl.Add(downloadFileNameEl);
            }

            XElement downloadVideoQualityEl = new XElement(DOWNLOAD_VIDEOQUALITY_EL);
            downloadVideoQualityEl.SetValue(preferences.DownloadVideoQuality);
            downloadEl.Add(downloadVideoQualityEl);

            string appDataFolder = this.folderService.GetAppDataFolder();

            FileSystem.CreateDirectory(appDataFolder);

            string configFile = Path.Combine(appDataFolder, CONFIG_FILE);

            doc.Save(configFile);

            this.currentPreferences = preferences;

            this.eventAggregator.GetEvent<PreferencesSavedEvent>().Publish();
        }

        private Preferences Load()
        {
            string configFile = Path.Combine(this.folderService.GetAppDataFolder(), CONFIG_FILE);

            Preferences preferences = this.CreateDefault();

            if (File.Exists(configFile))
            {
                XDocument doc = XDocument.Load(configFile);

                XElement preferencesEl = doc.Root;

                if (preferencesEl != null)
                {
                    XElement appEl = preferencesEl.Element(APP_EL);

                    if (appEl != null)
                    {
                        XElement appCheckForUpdatesEl = appEl.Element(APP_CHECKFORUPDATES_EL);

                        if (appCheckForUpdatesEl != null)
                        {
                            preferences.AppCheckForUpdates = appCheckForUpdatesEl.GetValueAsBool();
                        }
                    }

                    XElement searchEl = preferencesEl.Element(SEARCH_EL);

                    if (searchEl != null)
                    {
                        XElement searchChannelNameEl = searchEl.Element(SEARCH_CHANNELNAME_EL);

                        if (searchChannelNameEl != null)
                        {
                            preferences.SearchChannelName = searchChannelNameEl.GetValueAsString();
                        }

                        XElement searchVideoTypeEl = searchEl.Element(SEARCH_VIDEOTYPE_EL);

                        if (searchVideoTypeEl != null)
                        {
                            preferences.SearchVideoType = searchVideoTypeEl.GetValueAsEnum<VideoType>();
                        }

                        XElement searchLoadLimitEl = searchEl.Element(SEARCH_LOADLIMIT_EL);

                        if (searchLoadLimitEl != null)
                        {
                            preferences.SearchLoadLimit = searchLoadLimitEl.GetValueAsInt();
                        }

                        XElement searchOnStartupEl = searchEl.Element(SEARCH_SEARCHONSTARTUP_EL);

                        if (searchOnStartupEl != null)
                        {
                            preferences.SearchOnStartup = searchOnStartupEl.GetValueAsBool();
                        }
                    }

                    XElement downloadEl = preferencesEl.Element(DOWNLOAD_EL);

                    if (downloadEl != null)
                    {
                        XElement downloadFolderEl = downloadEl.Element(DOWNLOAD_FOLDER_EL);

                        if (downloadFolderEl != null)
                        {
                            preferences.DownloadFolder = downloadFolderEl.GetValueAsString();
                        }

                        XElement downloadFileNameEl = downloadEl.Element(DOWNLOAD_FILENAME_EL);

                        if (downloadFileNameEl != null)
                        {
                            preferences.DownloadFileName = downloadFileNameEl.GetValueAsString();
                        }

                        XElement downloadVideoQualityEl = downloadEl.Element(DOWNLOAD_VIDEOQUALITY_EL);

                        if (downloadVideoQualityEl != null)
                        {
                            preferences.DownloadVideoQuality = downloadVideoQualityEl.GetValueAsEnum<VideoQuality>();
                        }
                    }
                }
            }

            return preferences;
        }

        public Preferences CreateDefault()
        {
            Preferences preferences = new Preferences()
            {
                AppCheckForUpdates = true,
                SearchChannelName = null,
                SearchVideoType = VideoType.Broadcast,
                SearchLoadLimit = 10,
                SearchOnStartup = false,
                DownloadFolder = this.folderService.GetDownloadFolder(),
                DownloadFileName = FilenameWildcards.DATE + "_" + FilenameWildcards.ID + "_" + FilenameWildcards.GAME + ".mp4",
                DownloadVideoQuality = VideoQuality.Source
            };

            return preferences;
        }

        #endregion Methods
    }
}