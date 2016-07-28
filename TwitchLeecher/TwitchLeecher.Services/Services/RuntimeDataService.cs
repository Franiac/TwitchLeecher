using System.IO;
using System.Xml.Linq;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Extensions;
using TwitchLeecher.Shared.IO;

namespace TwitchLeecher.Services.Services
{
    internal class RuntimeDataService : IRuntimeDataService
    {
        #region Constants

        private const string RUNTIMEDATA_FILE = "runtime.xml";

        private const string RUNTIMEDATA_EL = "RuntimeData";

        private const string AUTH_EL = "Authorization";
        private const string AUTH_ACCESSTOKEN_EL = "AccessToken";

        #endregion Constants

        #region Fields

        private IFolderService folderService;

        private RuntimeData runtimeData;

        private readonly object commandLockObject;

        #endregion Fields

        #region Constructors

        public RuntimeDataService(IFolderService folderService)
        {
            this.folderService = folderService;

            this.commandLockObject = new object();
        }

        #endregion Constructors

        #region Properties

        public RuntimeData RuntimeData
        {
            get
            {
                if (this.runtimeData == null)
                {
                    this.runtimeData = this.Load();
                }

                return this.runtimeData;
            }
        }

        #endregion Properties

        #region Methods

        public void Save()
        {
            lock (this.commandLockObject)
            {
                RuntimeData runtimeData = this.RuntimeData;

                XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", null));

                XElement runtimeDataEl = new XElement(RUNTIMEDATA_EL);
                doc.Add(runtimeDataEl);

                if (!string.IsNullOrWhiteSpace(runtimeData.AccessToken))
                {
                    XElement authEl = new XElement(AUTH_EL);
                    runtimeDataEl.Add(authEl);

                    XElement accessTokenEl = new XElement(AUTH_ACCESSTOKEN_EL);
                    accessTokenEl.SetValue(this.runtimeData.AccessToken);
                    authEl.Add(accessTokenEl);
                }

                string appDataFolder = this.folderService.GetAppDataFolder();

                FileSystem.CreateDirectory(appDataFolder);

                string configFile = Path.Combine(appDataFolder, RUNTIMEDATA_FILE);

                doc.Save(configFile);
            }
        }

        private RuntimeData Load()
        {
            lock (this.commandLockObject)
            {
                string configFile = Path.Combine(this.folderService.GetAppDataFolder(), RUNTIMEDATA_FILE);

                RuntimeData runtimeData = new RuntimeData();

                if (File.Exists(configFile))
                {
                    XDocument doc = XDocument.Load(configFile);

                    XElement runtimedataEl = doc.Root;

                    if (runtimedataEl != null)
                    {
                        XElement authEl = runtimedataEl.Element(AUTH_EL);

                        if (authEl != null)
                        {
                            XElement accessTokenEl = authEl.Element(AUTH_ACCESSTOKEN_EL);

                            if (accessTokenEl != null)
                            {
                                try
                                {
                                    runtimeData.AccessToken = accessTokenEl.GetValueAsString();
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