using System;
using System.IO;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.IO;

namespace TwitchLeecher.Services.Services
{
    internal class LogService : ILogService
    {
        #region Constants

        private const string LOGS_FOLDER_NAME = "logs";

        #endregion Constants

        #region Fields

        private readonly string _logDir;

        #endregion Fields

        #region Constructors

        public LogService(IFolderService folderService)
        {
            if (folderService == null)
            {
                throw new ArgumentNullException(nameof(folderService));
            }

            _logDir = Path.Combine(folderService.GetAppDataFolder(), LOGS_FOLDER_NAME);
        }

        #endregion Constructors

        #region Methods

        public string LogException(Exception ex)
        {
            try
            {
                FileSystem.CreateDirectory(_logDir);

                string logFile = Path.Combine(_logDir, DateTime.UtcNow.ToString("MMddyyyy_hhmmss_fff_tt") + "_error.log");

                File.WriteAllText(logFile, ex.ToString());

                return logFile;
            }
            catch
            {
                // Do not crash application if logging fails
            }

            return null;
        }

        #endregion Methods
    }
}