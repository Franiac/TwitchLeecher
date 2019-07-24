using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TwitchLeecher.Core.Enums;
using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Events;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.Helpers;
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
        private const string APP_SHOWDONATIONBUTTON_EL = "ShowDonationButton";

        private const string SEARCH_EL = "Search";
        private const string SEARCH_FAVCHANNELS_EL = "FavChannels";
        private const string SEARCH_CHANNELNAME_EL = "ChannelName";
        private const string SEARCH_VIDEOTYPE_EL = "VideoType";
        private const string SEARCH_LOADLIMITTYPE_EL = "LoadLimitType";
        private const string SEARCH_LOADLASTDAYS_EL = "LoadLastDays";
        private const string SEARCH_LOADLASTVODS_EL = "LoadLastVods";
        private const string SEARCH_SEARCHONSTARTUP_EL = "SearchOnStartup";

        private const string DOWNLOAD_EL = "Download";
        private const string DOWNLOAD_TEMPFOLDER_EL = "TempFolder";
        private const string DOWNLOAD_FOLDER_EL = "Folder";
        private const string DOWNLOAD_FILENAME_EL = "FileName";
        private const string DOWNLOAD_SUBFOLDERSFORFAV_EL = "SubfoldersForFav";
        private const string DOWNLOAD_REMOVECOMPLETED_EL = "RemoveCompleted";
        private const string DOWNLOAD_DISABLECONVERSION_EL = "DisableConversion";

        private const string MISC_EL = "Misc";
        private const string MISC_USEEXTERNALPLAYER_EL = "UseExternalPlayer";
        private const string MISC_EXTERNALPLAYER_EL = "ExternalPlayer";

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

        public bool IsChannelInFavourites(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                return false;
            }

            string searchChannelName = CurrentPreferences.SearchChannelName;

            if (!string.IsNullOrWhiteSpace(searchChannelName) && searchChannelName.Equals(channel, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string existingEntry = CurrentPreferences.SearchFavouriteChannels.FirstOrDefault(c => c.Equals(channel, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(existingEntry))
            {
                return true;
            }

            return false;
        }

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

                XElement miscEl = new XElement(MISC_EL);
                preferencesEl.Add(miscEl);

                // Application
                XElement appCheckForUpdatesEl = new XElement(APP_CHECKFORUPDATES_EL);
                appCheckForUpdatesEl.SetValue(preferences.AppCheckForUpdates);
                appEl.Add(appCheckForUpdatesEl);

                XElement appShowDonationButtonEl = new XElement(APP_SHOWDONATIONBUTTON_EL);
                appShowDonationButtonEl.SetValue(preferences.AppShowDonationButton);
                appEl.Add(appShowDonationButtonEl);

                // Search
                RangeObservableCollection<string> favChannels = preferences.SearchFavouriteChannels;

                if (favChannels != null && favChannels.Count > 0)
                {
                    XElement favChannelsEl = new XElement(SEARCH_FAVCHANNELS_EL);
                    favChannelsEl.SetValue(string.Join(";", preferences.SearchFavouriteChannels));
                    searchEl.Add(favChannelsEl);
                }

                if (!string.IsNullOrWhiteSpace(preferences.SearchChannelName))
                {
                    XElement searchChannelNameEl = new XElement(SEARCH_CHANNELNAME_EL);
                    searchChannelNameEl.SetValue(preferences.SearchChannelName);
                    searchEl.Add(searchChannelNameEl);
                }

                XElement searchVideoTypeEl = new XElement(SEARCH_VIDEOTYPE_EL);
                searchVideoTypeEl.SetValue(preferences.SearchVideoType);
                searchEl.Add(searchVideoTypeEl);

                XElement searchLoadLimitTypeEl = new XElement(SEARCH_LOADLIMITTYPE_EL);
                searchLoadLimitTypeEl.SetValue(preferences.SearchLoadLimitType);
                searchEl.Add(searchLoadLimitTypeEl);

                XElement searchLoadLastDaysEl = new XElement(SEARCH_LOADLASTDAYS_EL);
                searchLoadLastDaysEl.SetValue(preferences.SearchLoadLastDays);
                searchEl.Add(searchLoadLastDaysEl);

                XElement searchLoadLastVodsEl = new XElement(SEARCH_LOADLASTVODS_EL);
                searchLoadLastVodsEl.SetValue(preferences.SearchLoadLastVods);
                searchEl.Add(searchLoadLastVodsEl);

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

                XElement downloadSubfoldersForFavEl = new XElement(DOWNLOAD_SUBFOLDERSFORFAV_EL);
                downloadSubfoldersForFavEl.SetValue(preferences.DownloadSubfoldersForFav);
                downloadEl.Add(downloadSubfoldersForFavEl);

                XElement downloadRemoveCompletedEl = new XElement(DOWNLOAD_REMOVECOMPLETED_EL);
                downloadRemoveCompletedEl.SetValue(preferences.DownloadRemoveCompleted);
                downloadEl.Add(downloadRemoveCompletedEl);

                XElement downloadDisableConversionEl = new XElement(DOWNLOAD_DISABLECONVERSION_EL);
                downloadDisableConversionEl.SetValue(preferences.DownloadDisableConversion);
                downloadEl.Add(downloadDisableConversionEl);

                // Miscellanious
                XElement miscUseExternalPlayerEl = new XElement(MISC_USEEXTERNALPLAYER_EL);
                miscUseExternalPlayerEl.SetValue(preferences.MiscUseExternalPlayer);
                miscEl.Add(miscUseExternalPlayerEl);

                if (!string.IsNullOrWhiteSpace(preferences.MiscExternalPlayer))
                {
                    XElement miscExternalPlayerEl = new XElement(MISC_EXTERNALPLAYER_EL);
                    miscExternalPlayerEl.SetValue(preferences.MiscExternalPlayer);
                    miscEl.Add(miscExternalPlayerEl);
                }

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

                            XElement appShowDonationButtonEl = appEl.Element(APP_SHOWDONATIONBUTTON_EL);

                            if (appShowDonationButtonEl != null)
                            {
                                try
                                {
                                    preferences.AppShowDonationButton = appShowDonationButtonEl.GetValueAsBool();
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
                            XElement searchFavChannelsEl = searchEl.Element(SEARCH_FAVCHANNELS_EL);

                            if (searchFavChannelsEl != null)
                            {
                                try
                                {
                                    string favChannelsStr = searchFavChannelsEl.GetValueAsString();

                                    if (!string.IsNullOrWhiteSpace(favChannelsStr))
                                    {
                                        string[] channelList = favChannelsStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                                        if (channelList.Length > 0)
                                        {
                                            preferences.SearchFavouriteChannels.AddRange(channelList);
                                        }
                                    }
                                }
                                catch
                                {
                                    // Value from config file could not be parsed
                                }
                            }

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

                            XElement searchLoadLimitTypeEl = searchEl.Element(SEARCH_LOADLIMITTYPE_EL);

                            if (searchLoadLimitTypeEl != null)
                            {
                                try
                                {
                                    preferences.SearchLoadLimitType = searchLoadLimitTypeEl.GetValueAsEnum<LoadLimitType>();
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }

                            XElement searchLoadLastDaysEl = searchEl.Element(SEARCH_LOADLASTDAYS_EL);

                            if (searchLoadLastDaysEl != null)
                            {
                                try
                                {
                                    preferences.SearchLoadLastDays = searchLoadLastDaysEl.GetValueAsInt();
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }

                            XElement searchLoadLastVodsEl = searchEl.Element(SEARCH_LOADLASTVODS_EL);

                            if (searchLoadLastVodsEl != null)
                            {
                                try
                                {
                                    preferences.SearchLoadLastVods = searchLoadLastVodsEl.GetValueAsInt();
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
                                    string fileName = downloadFileNameEl.GetValueAsString();

                                    if (preferences.Version < new Version(1, 6, 0) && !string.IsNullOrEmpty(fileName) && fileName.EndsWith(".mp4"))
                                    {
                                        fileName = fileName.Substring(0, fileName.Length - 4);
                                    }

                                    preferences.DownloadFileName = fileName;
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }

                            XElement donwloadSubfolderForFavEl = downloadEl.Element(DOWNLOAD_SUBFOLDERSFORFAV_EL);

                            if (donwloadSubfolderForFavEl != null)
                            {
                                try
                                {
                                    preferences.DownloadSubfoldersForFav = donwloadSubfolderForFavEl.GetValueAsBool();
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

                            XElement donwloadDisableConversionEl = downloadEl.Element(DOWNLOAD_DISABLECONVERSION_EL);

                            if (donwloadDisableConversionEl != null)
                            {
                                try
                                {
                                    preferences.DownloadDisableConversion = donwloadDisableConversionEl.GetValueAsBool();
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }
                        }

                        XElement miscEl = preferencesEl.Element(MISC_EL);

                        if (miscEl != null)
                        {
                            XElement miscUseExternalPlayerEl = miscEl.Element(MISC_USEEXTERNALPLAYER_EL);

                            if (miscUseExternalPlayerEl != null)
                            {
                                try
                                {
                                    preferences.MiscUseExternalPlayer = miscUseExternalPlayerEl.GetValueAsBool();
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }

                            XElement miscExternalPlayerEl = miscEl.Element(MISC_EXTERNALPLAYER_EL);

                            if (miscExternalPlayerEl != null)
                            {
                                try
                                {
                                    preferences.MiscExternalPlayer = miscExternalPlayerEl.GetValueAsString();
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
                AppShowDonationButton = true,
                SearchChannelName = null,
                SearchVideoType = VideoType.Broadcast,
                SearchLoadLimitType = LoadLimitType.Timespan,
                SearchLoadLastDays = 10,
                SearchLoadLastVods = 10,
                SearchOnStartup = false,
                DownloadTempFolder = _folderService.GetTempFolder(),
                DownloadFolder = _folderService.GetDownloadFolder(),
                DownloadFileName = FilenameWildcards.DATE + "_" + FilenameWildcards.ID + "_" + FilenameWildcards.GAME,
                DownloadRemoveCompleted = false,
                DownloadDisableConversion = false,
                MiscUseExternalPlayer = false,
                MiscExternalPlayer = null
            };

            return preferences;
        }

        #endregion Methods
    }
}