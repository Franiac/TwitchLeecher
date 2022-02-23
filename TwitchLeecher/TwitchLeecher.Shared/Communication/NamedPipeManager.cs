using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchLeecher.Shared.Communication
{
    public class NamedPipeManager
    {
        #region Constants

        private const string SHUTDOWN_MESSAGE = "$$SHUTDOWN$$";

        #endregion Constants

        #region Fields

        private readonly string _pipeName;

        private bool _started;

        private Task _worker;

        private CancellationTokenSource _cancellationTokenSource;

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
            if (!_started)
            {
                _started = true;

                _cancellationTokenSource = new CancellationTokenSource();

                CancellationToken cancellationToken = _cancellationTokenSource.Token;

                _worker = Task.Run(() =>
                {
                    while (true)
                    {
                        string text;

                        using (NamedPipeServerStream server = new NamedPipeServerStream(_pipeName))
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

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                }, cancellationToken);
            }
        }

        public void StopServer()
        {
            if (_started)
            {
                _started = false;

                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                }

                Write(SHUTDOWN_MESSAGE);

                if (_worker != null)
                {
                    _worker.Wait();
                }
            }
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