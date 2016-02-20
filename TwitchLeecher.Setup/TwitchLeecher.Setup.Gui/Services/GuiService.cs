using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace TwitchLeecher.Setup.Gui.Services
{
    internal class GuiService : IGuiService
    {
        #region Fields

        private Dispatcher dispatcher;
        private SetupApplication bootstrapper;

        private bool busy;

        #endregion Fields

        #region Constructors

        internal GuiService(SetupApplication bootstrapper, Dispatcher dispatcher)
        {
            if (bootstrapper == null)
            {
                throw new ArgumentNullException("bootstrapper");
            }

            if (dispatcher == null)
            {
                throw new ArgumentNullException("dispatcher");
            }

            this.bootstrapper = bootstrapper;
            this.dispatcher = dispatcher;
        }

        #endregion Constructors

        #region Methods

        public void SetBusy()
        {
            this.SetBusy(true);
        }

        private void SetBusy(bool busy)
        {
            if (this.busy != busy)
            {
                this.busy = busy;

                Mouse.OverrideCursor = busy ? Cursors.Wait : null;

                if (this.busy)
                {
                    new DispatcherTimer(TimeSpan.FromSeconds(0), DispatcherPriority.ApplicationIdle, dispatcherTimer_Tick, this.dispatcher);
                }
            }
        }

        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            return this.bootstrapper.InvokeOnUiThread<MessageBoxResult>(() =>
            {
                return MessageBox.Show(message, caption, button, image);
            });
        }

        #endregion Methods

        #region EventHandler

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer dispatcherTimer = sender as DispatcherTimer;

            if (dispatcherTimer != null)
            {
                SetBusy(false);
                dispatcherTimer.Stop();
            }
        }

        #endregion EventHandler
    }
}