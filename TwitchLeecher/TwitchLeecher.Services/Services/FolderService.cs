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
            if (string.IsNullOrWhiteSpace(appDataFolder))
            {
                string productName = AssemblyUtil.Get.GetProductName();
                appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), productName);
            }

            return appDataFolder;
        }

        public string GetTempFolder()
        {
            if (string.IsNullOrWhiteSpace(downloadsTempFolder))
            {
                downloadsTempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
            }

            return downloadsTempFolder;
        }

        public string GetDownloadFolder()
        {
            if (string.IsNullOrWhiteSpace(downloadsFolder))
            {
                downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }

            return downloadsFolder;
        }

        #endregion Methods
    }
}