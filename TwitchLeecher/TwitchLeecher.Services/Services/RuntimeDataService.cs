using System;
using System.IO;
using System.Xml.Linq;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.IO;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Services.Services
{
    internal class RuntimeDataService : IRuntimeDataService
    {
        #region Constants

        private const string RUNTIMEDATA_FILE = "runtime.xml";

        private const string RUNTIMEDATA_EL = "RuntimeData";
        private const string RUNTIMEDATA_VERSION_ATTR = "Version";

        private const string APP_EL = "Application";

        #endregion Constants

        #region Fields

        private IFolderService _folderService;

        private RuntimeData _runtimeData;
        private Version _tlVersion;

        private readonly object _commandLockObject;

        #endregion Fields

        #region Constructors

        public RuntimeDataService(IFolderService folderService)
        {
            _folderService = folderService;
            _tlVersion = AssemblyUtil.Get.GetAssemblyVersion().Trim();
            _commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public RuntimeData RuntimeData
        {
            get
            {
                if (_runtimeData == null)
                {
                    _runtimeData = Load();
                }

                return _runtimeData;
            }
        }

        #endregion Properties

        #region Methods

        public void Save()
        {
            lock (_commandLockObject)
            {
                RuntimeData runtimeData = RuntimeData;

                XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", null));

                XElement runtimeDataEl = new XElement(RUNTIMEDATA_EL);
                runtimeDataEl.Add(new XAttribute(RUNTIMEDATA_VERSION_ATTR, _tlVersion));
                doc.Add(runtimeDataEl);

                if (runtimeData.MainWindowInfo != null)
                {
                    XElement mainWindowInfoEl = runtimeData.MainWindowInfo.GetXml();

                    if (mainWindowInfoEl.HasElements)
                    {
                        XElement applicationEl = new XElement(APP_EL);
                        applicationEl.Add(mainWindowInfoEl);
                        runtimeDataEl.Add(applicationEl);
                    }
                }

                string appDataFolder = _folderService.GetAppDataFolder();

                FileSystem.CreateDirectory(appDataFolder);

                string configFile = Path.Combine(appDataFolder, RUNTIMEDATA_FILE);

                doc.Save(configFile);
            }
        }

        private RuntimeData Load()
        {
            lock (_commandLockObject)
            {
                string configFile = Path.Combine(_folderService.GetAppDataFolder(), RUNTIMEDATA_FILE);

                RuntimeData runtimeData = new RuntimeData()
                {
                    Version = _tlVersion
                };

                if (File.Exists(configFile))
                {
                    XDocument doc = XDocument.Load(configFile);

                    XElement runtimeDataEl = doc.Root;

                    if (runtimeDataEl != null)
                    {
                        XAttribute rtVersionAttr = runtimeDataEl.Attribute(RUNTIMEDATA_VERSION_ATTR);

                        if (rtVersionAttr != null && Version.TryParse(rtVersionAttr.Value, out Version rtVersion))
                        {
                            runtimeData.Version = rtVersion;
                        }
                        else
                        {
                            runtimeData.Version = new Version(1, 0);
                        }

                        XElement applicationEl = runtimeDataEl.Element(APP_EL);

                        if (applicationEl != null)
                        {
                            XElement mainWindowInfoEl = applicationEl.Element(MainWindowInfo.MAINWINDOW_EL);

                            if (mainWindowInfoEl != null)
                            {
                                try
                                {
                                    runtimeData.MainWindowInfo = MainWindowInfo.GetFromXml(mainWindowInfoEl);
                                }
                                catch
                                {
                                    // Value from config file could not be loaded, use default value
                                }
                            }
                        }
                    }
                }

                return runtimeData;
            }
        }

        #endregion Methods
    }
}