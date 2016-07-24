using Ninject;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.Views;

namespace TwitchLeecher.Gui.Services
{
    internal class NotificationService : INotificationService
    {
        #region Fields

        private IKernel kernel;

        #endregion Fields

        #region Constructor

        public NotificationService(IKernel kernel)
        {
            this.kernel = kernel;
        }

        #endregion Constructor

        #region Methods

        public void ShowNotification(string text)
        {
            this.kernel.Get<MainWindow>().ShowNotification(text);
        }

        #endregion Methods
    }
}