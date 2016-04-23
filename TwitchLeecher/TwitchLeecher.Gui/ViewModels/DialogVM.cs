using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Gui.ViewModels
{
    public abstract class DialogVM<T> : ViewModelBase
    {
        #region Properties

        public T ResultObject { get; protected set; }

        #endregion Properties
    }
}