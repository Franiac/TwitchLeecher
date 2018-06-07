using System;
using System.Collections.Generic;

namespace TwitchLeecher.Shared.Events
{
    public class EmptyPresentationEvent : EventBase
    {
        #region Fields

        private readonly PubSubEvent<object> _innerEvent;
        private readonly Dictionary<Action, Action<object>> _subscriberActions;

        #endregion Fields

        #region Constructors

        public EmptyPresentationEvent()
        {
            _innerEvent = new PubSubEvent<object>();
            _subscriberActions = new Dictionary<Action, Action<object>>();
        }

        #endregion Constructors

        #region Methods

        public void Publish()
        {
            _innerEvent.Publish(null);
        }

        public void Subscribe(Action action)
        {
            Subscribe(action, false);
        }

        public void Subscribe(Action action, bool keepSubscriberReferenceAlive)
        {
            Subscribe(action, ThreadOption.PublisherThread, keepSubscriberReferenceAlive);
        }

        public void Subscribe(Action action, ThreadOption threadOption)
        {
            Subscribe(action, threadOption, false);
        }

        public void Subscribe(Action action, ThreadOption threadOption, bool keepSubscriberReferenceAlive)
        {
            void wrappedAction(object o)
            {
                action();
            }

            _subscriberActions.Add(action, wrappedAction);
            _innerEvent.Subscribe(wrappedAction, threadOption, keepSubscriberReferenceAlive);
        }

        public void Unsubscribe(Action action)
        {
            if (!_subscriberActions.ContainsKey(action))
            {
                return;
            }

            Action<object> wrappedActionToUnsubscribe = _subscriberActions[action];
            _innerEvent.Unsubscribe(wrappedActionToUnsubscribe);
            _subscriberActions.Remove(action);
        }

        #endregion Methods
    }
}