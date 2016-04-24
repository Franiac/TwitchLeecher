using System;
using System.IO;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Reflection;

namespace TwitchLeecher.Services.Services
{
    internal class FolderService : IFolderService
    {
        #region Fields

        private string appDataFolder;
        private string downloadsTempFolder;
        private string downloadsFolder;

        #endregion Fields

        #region Methods

        public string GetAppDataFolder()
        {
            if (string.IsNullOrWhiteSpace(this.appDataFolder))
            {
                string productName = AssemblyUtil.Get.GetProductName();
                this.appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), productName);
            }

            return this.appDataFolder;
        }

        public string GetTempFolder()
        {
            if (string.IsNullOrWhiteSpace(this.downloadsTempFolder))
            {
                this.downloadsTempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
            }

            return this.downloadsTempFolder;
        }

        public string GetDownloadFolder()
        {
            if (string.IsNullOrWhiteSpace(this.downloadsFolder))
            {
                this.downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }

            return this.downloadsFolder;
        }

        #endregion Methods
    }
}