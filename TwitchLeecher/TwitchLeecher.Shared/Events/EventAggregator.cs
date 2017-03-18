using System;
using System.Collections.Generic;
using System.Threading;

namespace TwitchLeecher.Shared.Events
{
    public class EventAggregator : IEventAggregator
    {
        #region Fields

        private readonly Dictionary<Type, EventBase> _events;
        private readonly SynchronizationContext _syncContext;

        #endregion Fields

        #region Constructors

        public EventAggregator()
        {
            _events = new Dictionary<Type, EventBase>();
            _syncContext = SynchronizationContext.Current;
        }

        #endregion Constructors

        #region Methods

        public TEventType GetEvent<TEventType>() where TEventType : EventBase, new()
        {
            lock (_events)
            {
                if (!_events.TryGetValue(typeof(TEventType), out EventBase existingEvent))
                {
                    TEventType newEvent = new TEventType();
                    newEvent.SynchronizationContext = _syncContext;
                    _events[typeof(TEventType)] = newEvent;

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