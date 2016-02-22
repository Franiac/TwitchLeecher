namespace TwitchLeecher.Shared.Events
{
    public interface IEventAggregator
    {
        #region Methods

        TEventType GetEvent<TEventType>() where TEventType : EventBase, new();

        #endregion Methods
    }
}