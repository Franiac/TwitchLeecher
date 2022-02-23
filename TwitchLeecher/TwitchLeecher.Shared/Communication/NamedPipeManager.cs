using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace TwitchLeecher.Shared.Communication
{
    public class NamedPipeManager
    {
        #region Constants

        private const string SHUTDOWN_MESSAGE = "$$SHUTDOWN$$";

        #endregion Constants

        #region Fields

        private readonly string _pipeName;

        private bool _isRunning;

        private Thread _worker;

        #endregion Fields

        #region Events

        public event Action<string> OnMessage;

        #endregion Events

        #region Constructors

        public NamedPipeManager(string pipeName)
        {
            if (string.IsNullOrWhiteSpace(pipeName))
            {
                throw new ArgumentNullException(nameof(pipeName));
            }

            _pipeName = pipeName;
        }

        #endregion Constructors

        #region Methods

        public void StartServer()
        {
            _worker = new Thread((pipeName) =>
            {
                _isRunning = true;

                while (true)
                {
                    string text;

                    using (NamedPipeServerStream server = new NamedPipeServerStream(pipeName as string))
                    {
                        server.WaitForConnection();

                        using (StreamReader reader = new StreamReader(server))
                        {
                            text = reader.ReadToEnd();
                        }
                    }

                    if (text == SHUTDOWN_MESSAGE)
                    {
                        break;
                    }

                    OnMessage?.Invoke(text);

                    if (_isRunning == false)
                    {
                        break;
                    }
                }
            });

            _worker.Start(_pipeName);
        }

        public void StopServer()
        {
            _isRunning = false;
            Write(SHUTDOWN_MESSAGE);
            Thread.Sleep(100);
        }

        public bool Write(string text, int connectTimeout = 300)
        {
            using (NamedPipeClientStream client = new NamedPipeClientStream(_pipeName))
            {
                try
                {
                    client.Connect(connectTimeout);
                }
                catch
                {
                    return false;
                }

                if (!client.IsConnected)
                {
                    return false;
                }

                using (StreamWriter writer = new StreamWriter(client))
                {
                    writer.Write(text);
                    writer.Flush();
                }
            }

            return true;
        }

        #endregion Methods
    }
}