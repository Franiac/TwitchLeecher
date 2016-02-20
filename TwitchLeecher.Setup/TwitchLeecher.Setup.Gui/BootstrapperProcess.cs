using System;
using System.Diagnostics;

namespace TwitchLeecher.Setup.Gui
{
    internal class BootstrapperProcess : Process
    {
        #region Fields

        private string executionId;
        private Action<int, string> exitedCallback;

        #endregion Fields

        #region Constructors

        public BootstrapperProcess(string executionId, ProcessStartInfo psi, Action<int, string> exitedCallback = null)
        {
            if (string.IsNullOrWhiteSpace(executionId))
            {
                throw new ArgumentNullException("executionId");
            }

            if (psi == null)
            {
                throw new ArgumentNullException("psi");
            }

            this.executionId = executionId;
            this.StartInfo = psi;
            this.exitedCallback = exitedCallback;
        }

        #endregion Constructors

        #region Properties

        public string ExecutionId
        {
            get
            {
                return this.executionId;
            }
        }

        public Action<int, string> ExitedCallback
        {
            get
            {
                return this.exitedCallback;
            }
        }

        #endregion Properties
    }
}