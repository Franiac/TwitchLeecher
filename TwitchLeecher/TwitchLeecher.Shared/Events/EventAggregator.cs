using System;
using System.Collections.Generic;
using System.Threading;

namespace TwitchLeecher.Shared.Events
{
    public class EventAggregator : IEventAggregator
    {
        #region Fields

        private readonly Dictionary<Type, EventBase> events = new Dictionary<Type, EventBase>();
        private readonly SynchronizationContext syncContext = SynchronizationContext.Current;

        #endregion Fields

        #region Methods

        public TEventType GetEvent<TEventType>() where TEventType : EventBase, new()
        {
            lock (events)
            {
                EventBase existingEvent = null;

                if (!events.TryGetValue(typeof(TEventType), out existingEvent))
                {
                    TEventType newEvent = new TEventType();
                    newEvent.SynchronizationContext = syncContext;
                    events[typeof(TEventType)] = newEvent;

                    return newEvent;
                }
                else
                {
                    return (TEventType)existingEvent;
                }
            }
        }

        #endregion Methods
    }
}