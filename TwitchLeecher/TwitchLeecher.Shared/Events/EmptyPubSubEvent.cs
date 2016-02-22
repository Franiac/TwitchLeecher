using TwitchLeecher.Shared.Events;
using System;
using System.Collections.Generic;

namespace TwitchLeecher.Shared.Events
{
    public class EmptyPresentationEvent : EventBase
    {
        private readonly PubSubEvent<object> innerEvent;

        private readonly Dictionary<Action, Action<object>> subscriberActions;

        #region Constructors

        public EmptyPresentationEvent()
        {
            this.innerEvent = new PubSubEvent<object>();
            this.subscriberActions = new Dictionary<Action, Action<object>>();
        }

        #endregion Constructors

        #region Methods

        public void Publish()
        {
            this.innerEvent.Publish(null);
        }

        public void Subscribe(Action action)
        {
            this.Subscribe(action, false);
        }

        public void Subscribe(Action action, bool keepSubscriberReferenceAlive)
        {
            this.Subscribe(action, ThreadOption.PublisherThread, keepSubscriberReferenceAlive);
        }

        public void Subscribe(Action action, ThreadOption threadOption)
        {
            this.Subscribe(action, threadOption, false);
        }

        public void Subscribe(Action action, ThreadOption threadOption, bool keepSubscriberReferenceAlive)
        {
            Action<object> wrappedAction = o => action();
            this.subscriberActions.Add(action, wrappedAction);
            this.innerEvent.Subscribe(wrappedAction, threadOption, keepSubscriberReferenceAlive);
        }

        public void Unsubscribe(Action action)
        {
            if (!subscriberActions.ContainsKey(action))
            {
                return;
            }

            Action<object> wrappedActionToUnsubscribe = subscriberActions[action];
            this.innerEvent.Unsubscribe(wrappedActionToUnsubscribe);
            this.subscriberActions.Remove(action);
        }

        #endregion Methods
    }
}