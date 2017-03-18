using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TwitchLeecher.Shared.Events
{
    public abstract class EventBase
    {
        #region Fields

        private readonly List<IEventSubscription> _subscriptions = new List<IEventSubscription>();

        #endregion Fields

        #region Properties

        public SynchronizationContext SynchronizationContext { get; set; }

        protected ICollection<IEventSubscription> Subscriptions
        {
            get
            {
                return _subscriptions;
            }
        }

        #endregion Properties

        #region Methods

        public virtual bool Contains(SubscriptionToken token)
        {
            lock (Subscriptions)
            {
                return Subscriptions.FirstOrDefault(evt => evt.SubscriptionToken == token) != null;
            }
        }

        public virtual void Unsubscribe(SubscriptionToken token)
        {
            lock (Subscriptions)
            {
                IEventSubscription subscription = Subscriptions.FirstOrDefault(evt => evt.SubscriptionToken == token);

                if (subscription != null)
                {
                    Subscriptions.Remove(subscription);
                }
            }
        }

        protected virtual void InternalPublish(params object[] arguments)
        {
            List<Action<object[]>> executionStrategies = PruneAndReturnStrategies();

            foreach (var executionStrategy in executionStrategies)
            {
                executionStrategy(arguments);
            }
        }

        protected virtual SubscriptionToken InternalSubscribe(IEventSubscription eventSubscription)
        {
            if (eventSubscription == null)
            {
                throw new ArgumentNullException(nameof(eventSubscription));
            }

            eventSubscription.SubscriptionToken = new SubscriptionToken(Unsubscribe);

            lock (Subscriptions)
            {
                Subscriptions.Add(eventSubscription);
            }

            return eventSubscription.SubscriptionToken;
        }

        private List<Action<object[]>> PruneAndReturnStrategies()
        {
            List<Action<object[]>> returnList = new List<Action<object[]>>();

            lock (Subscriptions)
            {
                for (int i = Subscriptions.Count - 1; i >= 0; i--)
                {
                    Action<object[]> listItem = _subscriptions[i].GetExecutionStrategy();

                    if (listItem == null)
                    {
                        _subscriptions.RemoveAt(i);
                    }
                    else
                    {
                        returnList.Add(listItem);
                    }
                }
            }

            return returnList;
        }

        #endregion Methods
    }
}