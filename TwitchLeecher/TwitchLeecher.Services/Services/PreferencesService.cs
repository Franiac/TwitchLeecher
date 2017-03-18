using System;
using System.IO;
using System.Xml.Linq;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Events;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Services.Services
{
    internal class PreferencesService : IPreferencesService
    {
        #region Constants

        private const string CONFIG_FILE = "config.xml";

        private const string PREFERENCES_EL = "Preferences";
        private const string PREFERENCES_VERSION_ATTR = "Version";

        private const string APP_EL = "Application";
        private const string APP_CHECKFORUPDATES_EL = "CheckForUpdates";

        private const string SEARCH_EL = "Search";
        private const string SEARCH_CHANNELNAME_EL = "ChannelName";
        private const string SEARCH_VIDEOTYPE_EL = "VideoType";
        private const string SEARCH_LOADLIMIT_EL = "LoadLimit";
        private const string SEARCH_SEARCHONSTARTUP_EL = "SearchOnStartup";

        private const string DOWNLOAD_EL = "Download";
        private const string DOWNLOAD_TEMPFOLDER_EL = "TempFolder";
        private const string DOWNLOAD_FOLDER_EL = "Folder";
        private const string DOWNLOAD_FILENAME_EL = "FileName";
        private const string DOWNLOAD_VIDEOQUALITY_EL = "VideoQuality";
        private const string DOWNLOAD_REMOVECOMPLETED_EL = "RemoveCompleted";

        #endregion Constants

        #region Fields

        private IFolderService _folderService;
        private IEventAggregator _eventAggregator;

        private Preferences _currentPreferences;
        private Version _tlVersion;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructors

        public PreferencesService(IFolderService folderService, IEventAggregator eventAggregator)
        {
            _folderService = folderService;
            _eventAggregator = eventAggregator;

            _tlVersion = AssemblyUtil.Get.GetAssemblyVersion().Trim();
            _commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public Preferences CurrentPreferences
        {
            get
            {
                if (_currentPreferences == null)
                {
                    _currentPreferences = Load();
                }

                return _currentPreferences;
            }
        }

        #endregion Properties

        #region Methods

        public void Save(Preferences preferences)
        {
            lock (_commandLockObject)
            {
                XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", null));

                XElement preferencesEl = new XElement(PREFERENCES_EL);
                preferencesEl.Add(new XAttribute(PREFERENCES_VERSION_ATTR, _tlVersion));
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
                if (!string.IsNullOrWhiteSpace(preferences.DownloadTempFolder))
                {
                    XElement downloadTempFolderEl = new XElement(DOWNLOAD_TEMPFOLDER_EL);
                    downloadTempFolderEl.SetValue(preferences.DownloadTempFolder);
                    downloadEl.Add(downloadTempFolderEl);
                }

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

                XElement downloadRemoveCompletedEl = new XElement(DOWNLOAD_REMOVECOMPLETED_EL);
                downloadRemoveCompletedEl.SetValue(preferences.DownloadRemoveCompleted);
                downloadEl.Add(downloadRemoveCompletedEl);

                string appDataFolder = _folderService.GetAppDataFolder();

                FileSystem.CreateDirectory(appDataFolder);

                string configFile = Path.Combine(appDataFolder, CONFIG_FILE);

                doc.Save(configFile);

                _currentPreferences = preferences;

                _eventAggregator.GetEvent<PreferencesSavedEvent>().Publish();
            }
        }

        private Preferences Load()
        {
            lock (_commandLockObject)
            {
                string configFile = Path.Combine(_folderService.GetAppDataFolder(), CONFIG_FILE);

                Preferences preferences = CreateDefault();

                if (File.Exists(configFile))
                {
                    XDocument doc = XDocument.Load(configFile);

                    XElement preferencesEl = doc.Root;

                    if (preferencesEl != null)
                    {
                        XAttribute prefVersionAttr = preferencesEl.Attribute(PREFERENCES_VERSION_ATTR);


                        if (prefVersionAttr != null && Version.TryParse(prefVersionAttr.Value, out Version prefVersion))
                        {
                            preferences.Version = prefVersion;
                        }
                        else
                        {
                            preferences.Version = new Version(1, 0);
                        }

                        XElement appEl = preferencesEl.Element(APP_EL);

                        if (appEl != null)
                        {
                            XElement appCheckForUpdatesEl = appEl.Element(APP_CHECKFORUPDATES_EL);

                            if (appCheckForUpdatesEl != null)
                            {
                                try
                                {
                                    preferences.AppCheckForUpdates = appCheckForUpdatesEl.GetValueAsBool();
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }
                        }

                        XElement searchEl = preferencesEl.Element(SEARCH_EL);

                        if (searchEl != null)
                        {
                            XElement searchChannelNameEl = searchEl.Element(SEARCH_CHANNELNAME_EL);

                            if (searchChannelNameEl != null)
                            {
                                try
                                {
                                    preferences.SearchChannelName = searchChannelNameEl.GetValueAsString();
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }

                            XElement searchVideoTypeEl = searchEl.Element(SEARCH_VIDEOTYPE_EL);

                            if (searchVideoTypeEl != null)
                            {
                                try
                                {
                                    preferences.SearchVideoType = searchVideoTypeEl.GetValueAsEnum<VideoType>();
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }

                            XElement searchLoadLimitEl = searchEl.Element(SEARCH_LOADLIMIT_EL);

                            if (searchLoadLimitEl != null)
                            {
                                try
                                {
                                    int limit = searchLoadLimitEl.GetValueAsInt();

                                    if (Preferences.GetLoadLimits().Contains(limit))
                                    {
                                        preferences.SearchLoadLimit = limit;
                                    }
                                    else
                                    {
                                        preferences.SearchLoadLimit = Preferences.DEFAULT_LOAD_LIMIT;
                                    }
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }

                            XElement searchOnStartupEl = searchEl.Element(SEARCH_SEARCHONSTARTUP_EL);

                            if (searchOnStartupEl != null)
                            {
                                try
                                {
                                    preferences.SearchOnStartup = searchOnStartupEl.GetValueAsBool();
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }
                        }

                        XElement downloadEl = preferencesEl.Element(DOWNLOAD_EL);

                        if (downloadEl != null)
                        {
                            XElement downloadTempFolderEl = downloadEl.Element(DOWNLOAD_TEMPFOLDER_EL);

                            if (downloadTempFolderEl != null)
                            {
                                try
                                {
                                    preferences.DownloadTempFolder = downloadTempFolderEl.GetValueAsString();
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }

                            XElement downloadFolderEl = downloadEl.Element(DOWNLOAD_FOLDER_EL);

                            if (downloadFolderEl != null)
                            {
                                try
                                {
                                    preferences.DownloadFolder = downloadFolderEl.GetValueAsString();
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }

                            XElement downloadFileNameEl = downloadEl.Element(DOWNLOAD_FILENAME_EL);

                            if (downloadFileNameEl != null)
                            {
                                try
                                {
                                    preferences.DownloadFileName = downloadFileNameEl.GetValueAsString();
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }

                            XElement downloadVideoQualityEl = downloadEl.Element(DOWNLOAD_VIDEOQUALITY_EL);

                            if (downloadVideoQualityEl != null)
                            {
                                try
                                {
                                    preferences.DownloadVideoQuality = downloadVideoQualityEl.GetValueAsString();
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }

                            XElement donwloadRemoveCompletedEl = downloadEl.Element(DOWNLOAD_REMOVECOMPLETED_EL);

                            if (donwloadRemoveCompletedEl != null)
                            {
                                try
                                {
                                    preferences.DownloadRemoveCompleted = donwloadRemoveCompletedEl.GetValueAsBool();
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }
                        }
                    }
                }

                return preferences;
            }
        }

        public Preferences CreateDefault()
        {
            Preferences preferences = new Preferences()
            {
                Version = _tlVersion,
                AppCheckForUpdates = true,
                SearchChannelName = null,
                SearchVideoType = VideoType.Broadcast,
                SearchLoadLimit = Preferences.DEFAULT_LOAD_LIMIT,
                SearchOnStartup = false,
                DownloadTempFolder = _folderService.GetTempFolder(),
                DownloadFolder = _folderService.GetDownloadFolder(),
                DownloadFileName = FilenameWildcards.DATE + "_" + FilenameWildcards.ID + "_" + FilenameWildcards.GAME + ".mp4",
                DownloadVideoQuality = TwitchVideoQuality.QUALITY_SOURCE,
                DownloadRemoveCompleted = false
            };

            return preferences;
        }

        #endregion Methods
    }
}