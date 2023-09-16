using Ninject;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.Views;

namespace TwitchLeecher.Gui.Services
{
    internal class NotificationService : INotificationService
    {
        #region Fields

        private readonly IKernel _kernel;

        #endregion Fields

        #region Constructor

        public NotificationService(IKernel kernel)
        {
            _kernel = kernel;
        }

        #endregion Constructor

        #region Methods

        public void ShowNotification(string text)
        {
            _kernel.Get<MainWindow>().ShowNotification(text);
        }

        #endregion Methods
    }
}