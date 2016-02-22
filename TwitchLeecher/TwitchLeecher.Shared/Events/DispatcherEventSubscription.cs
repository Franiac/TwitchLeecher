using System;
using System.Threading;

namespace TwitchLeecher.Shared.Events
{
    public class DispatcherEventSubscription<TPayload> : EventSubscription<TPayload>
    {
        #region Fields

        private readonly SynchronizationContext syncContext;

        #endregion Fields

        #region Constructors

        public DispatcherEventSubscription(IDelegateReference actionReference, IDelegateReference filterReference, SynchronizationContext syncContext)
             : base(actionReference, filterReference)
        {
            this.syncContext = syncContext;
        }

        #endregion Constructors

        #region Methods

        public override void InvokeAction(Action<TPayload> action, TPayload argument)
        {
            syncContext.Post((o) => action((TPayload)o), argument);
        }

        #endregion Methods
    }
}