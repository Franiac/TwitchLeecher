using TwitchLeecher.Shared.Notification;

namespace TwitchLeecher.Gui.ViewModels
{
    public abstract class ViewModelBase : BindableBase
    {
        public virtual void OnBeforeShown()
        {
        }

        public virtual void OnBeforeHidden()
        {
        }
    }
}