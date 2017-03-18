using System;
using System.Diagnostics;

namespace TwitchLeecher.Setup.Gui
{
    internal class BootstrapperProcess : Process
    {
        #region Fields

        private string _executionId;
        private Action<int, string> _exitedCallback;

        #endregion Fields

        #region Constructors

        public BootstrapperProcess(string executionId, ProcessStartInfo psi, Action<int, string> exitedCallback = null)
        {
            if (string.IsNullOrWhiteSpace(executionId))
            {
                throw new ArgumentNullException("executionId");
            }

            StartInfo = psi ?? throw new ArgumentNullException("psi");

            _executionId = executionId;            
            _exitedCallback = exitedCallback;
        }

        #endregion Constructors

        #region Properties

        public string ExecutionId
        {
            get
            {
                return _executionId;
            }
        }

        public Action<int, string> ExitedCallback
        {
            get
            {
                return _exitedCallback;
            }
        }

        #endregion Properties
    }
}