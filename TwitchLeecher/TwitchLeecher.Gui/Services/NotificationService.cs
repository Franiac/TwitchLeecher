using Ninject;
using TwitchLeecher.Gui.Interfaces;
using TwitchLeecher.Gui.ViewModels;
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
            var mainWindow = _kernel.Get<MainWindow>();
            var mainWindowViewModel = (MainWindowVM)mainWindow.DataContext;
            mainWindowViewModel.SetNotification(text);
        }

        #endregion Methods
    }
}