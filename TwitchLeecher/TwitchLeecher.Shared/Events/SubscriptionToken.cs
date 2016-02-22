using System;

namespace TwitchLeecher.Shared.Events
{
    public class SubscriptionToken : IEquatable<SubscriptionToken>, IDisposable
    {
        #region Fields

        private readonly Guid token;
        private Action<SubscriptionToken> unsubscribeAction;

        #endregion Fields

        #region Constructors

        public SubscriptionToken(Action<SubscriptionToken> unsubscribeAction)
        {
            this.unsubscribeAction = unsubscribeAction;
            token = Guid.NewGuid();
        }

        #endregion Constructors

        #region Methods

        public virtual void Dispose()
        {
            if (this.unsubscribeAction != null)
            {
                this.unsubscribeAction(this);
                this.unsubscribeAction = null;
            }

            GC.SuppressFinalize(this);
        }

        public bool Equals(SubscriptionToken other)
        {
            if (other == null)
            {
                return false;
            }

            return Equals(token, other.token);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as SubscriptionToken);
        }

        public override int GetHashCode()
        {
            return token.GetHashCode();
        }

        #endregion Methods
    }
}