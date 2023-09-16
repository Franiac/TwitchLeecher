using System;
using System.Diagnostics;

namespace TwitchLeecher.Setup.Gui
{
    internal class BootstrapperProcess : Process
    {
        #region Constructors

        public BootstrapperProcess(string executionId, ProcessStartInfo psi, Action<int, string> exitedCallback = null)
        {
            if (string.IsNullOrWhiteSpace(executionId))
            {
                throw new ArgumentNullException("executionId");
            }

            StartInfo = psi ?? throw new ArgumentNullException("psi");

            ExecutionId = executionId;
            ExitedCallback = exitedCallback;
        }

        #endregion Constructors

        #region Properties

        public string ExecutionId { get; }

        public Action<int, string> ExitedCallback { get; }

        #endregion Properties
    }
}