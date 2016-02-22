using System;
using System.Threading.Tasks;

namespace TwitchLeecher.Shared.Events
{
    public class BackgroundEventSubscription<TPayload> : EventSubscription<TPayload>
    {
        #region Constructors

        public BackgroundEventSubscription(IDelegateReference actionReference, IDelegateReference filterReference)
             : base(actionReference, filterReference)
        {
        }

        #endregion Constructors

        #region Methods

        public override void InvokeAction(Action<TPayload> action, TPayload argument)
        {
            Task.Run(() => action(argument));
        }

        #endregion Methods
    }
}