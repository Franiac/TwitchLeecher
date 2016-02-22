using System;

namespace TwitchLeecher.Shared.Events
{
    public interface IEventSubscription
    {
        #region Properties

        SubscriptionToken SubscriptionToken { get; set; }

        #endregion Properties

        #region Methods

        Action<object[]> GetExecutionStrategy();

        #endregion Methods
    }
}