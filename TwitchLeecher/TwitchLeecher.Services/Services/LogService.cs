using System;
using System.IO;
using System.Reflection;
using TwitchLeecher.Services.Interfaces;

namespace TwitchLeecher.Services.Services
{
    public class LogService : ILogService
    {
        #region Constants

        private const string LOGS_FOLDER_NAME = "Logs";

        #endregion Constants

        #region Fields

        private string logDir;

        #endregion Fields

        #region Constructors

        public LogService()
        {
            this.logDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), LOGS_FOLDER_NAME);
        }

        #endregion Constructors

        #region Methods

        public string LogException(Exception ex)
        {
            try
            {
                if (!Directory.Exists(this.logDir))
                {
                    Directory.CreateDirectory(this.logDir);
                }

                string logFile = Path.Combine(this.logDir, DateTime.UtcNow.ToString("MMddyyyy_hhmmss_fff_tt") + "_error.log");

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