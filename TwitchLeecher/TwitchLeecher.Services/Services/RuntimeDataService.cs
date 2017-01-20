using System.IO;
using System.Xml.Linq;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.IO;

namespace TwitchLeecher.Services.Services
{
    internal class RuntimeDataService : IRuntimeDataService
    {
        #region Constants

        private const string RUNTIMEDATA_FILE = "runtime.xml";

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

                XElement runtimeDataEl = runtimeData.GetXml();

                doc.Add(runtimeDataEl);

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
                    runtimeData = RuntimeData.GetFromXml(doc.Root);
                }

                return runtimeData;
            }
        }

        #endregion Methods
    }
}