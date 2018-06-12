using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace TwitchLeecher.Setup.Gui.Services
{
    internal class GuiService : IGuiService
    {
        #region Fields

        private readonly Dispatcher _dispatcher;
        private readonly SetupApplication _bootstrapper;

        private bool _busy;

        #endregion Fields

        #region Constructors

        internal GuiService(SetupApplication bootstrapper, Dispatcher dispatcher)
        {
            _bootstrapper = bootstrapper ?? throw new ArgumentNullException("bootstrapper");
            _dispatcher = dispatcher ?? throw new ArgumentNullException("dispatcher");
        }

        #endregion Constructors

        #region Methods

        public void SetBusy()
        {
            SetBusy(true);
        }

        private void SetBusy(bool busy)
        {
            if (_busy != busy)
            {
                _busy = busy;

                Mouse.OverrideCursor = busy ? Cursors.Wait : null;

                if (_busy)
                {
                    new DispatcherTimer(TimeSpan.FromSeconds(0), DispatcherPriority.ApplicationIdle, DispatcherTimer_Tick, _dispatcher);
                }
            }
        }

        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            return _bootstrapper.InvokeOnUiThread<MessageBoxResult>(() =>
            {
                return MessageBox.Show(message, caption, button, image);
            });
        }

        #endregion Methods

        #region EventHandler

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender is DispatcherTimer dispatcherTimer)
            {
                SetBusy(false);
                dispatcherTimer.Stop();
            }
        }

        #endregion EventHandler
    }
}