using System;

namespace TwitchLeecher.Shared.Events
{
    public class EventSubscription<TPayload> : IEventSubscription
    {
        #region Fields

        private readonly IDelegateReference _actionReference;
        private readonly IDelegateReference _filterReference;

        #endregion Fields

        #region Constructors

        public EventSubscription(IDelegateReference actionReference, IDelegateReference filterReference)
        {
            if (actionReference == null)
            {
                throw new ArgumentNullException(nameof(actionReference));
            }

            if (!(actionReference.Target is Action<TPayload>))
            {
                throw new ArgumentException("Invalid action reference target type", nameof(actionReference));
            }

            if (filterReference == null)
            {
                throw new ArgumentNullException(nameof(filterReference));
            }

            if (!(filterReference.Target is Predicate<TPayload>))
            {
                throw new ArgumentException("Invalid filter reference target type", nameof(filterReference));
            }

            _actionReference = actionReference;
            _filterReference = filterReference;
        }

        #endregion Constructors

        #region Properties

        public Action<TPayload> Action
        {
            get
            {
                return (Action<TPayload>)_actionReference.Target;
            }
        }

        public Predicate<TPayload> Filter
        {
            get
            {
                return (Predicate<TPayload>)_filterReference.Target;
            }
        }

        public SubscriptionToken SubscriptionToken { get; set; }

        #endregion Properties

        #region Methods

        public virtual Action<object[]> GetExecutionStrategy()
        {
            Action<TPayload> action = Action;
            Predicate<TPayload> filter = Filter;

            if (action != null && filter != null)
            {
                return arguments =>
                {
                    TPayload argument = default(TPayload);
                    if (arguments != null && arguments.Length > 0 && arguments[0] != null)
                    {
                        argument = (TPayload)arguments[0];
                    }
                    if (filter(argument))
                    {
                        InvokeAction(action, argument);
                    }
                };
            }

            return null;
        }

        public virtual void InvokeAction(Action<TPayload> action, TPayload argument)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            action(argument);
        }

        #endregion Methods
    }
}