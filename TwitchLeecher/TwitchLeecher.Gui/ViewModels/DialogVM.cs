namespace TwitchLeecher.Gui.ViewModels
{
    public abstract class DialogVM<T> : BaseVM
    {
        #region Properties

        public T ResultObject { get; protected set; }

        #endregion Properties
    }
}