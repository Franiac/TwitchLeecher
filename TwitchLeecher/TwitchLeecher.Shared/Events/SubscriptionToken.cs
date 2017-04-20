using System;

namespace TwitchLeecher.Shared.Events
{
    public sealed class SubscriptionToken : IEquatable<SubscriptionToken>, IDisposable
    {
        #region Fields

        private readonly Guid _token;
        private Action<SubscriptionToken> _unsubscribeAction;

        #endregion Fields

        #region Constructors

        public SubscriptionToken(Action<SubscriptionToken> unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction;
            _token = Guid.NewGuid();
        }

        #endregion Constructors

        #region Methods

        public void Dispose()
        {
            if (_unsubscribeAction != null)
            {
                _unsubscribeAction(this);
                _unsubscribeAction = null;
            }

            GC.SuppressFinalize(this);
        }

        public bool Equals(SubscriptionToken other)
        {
            if (other == null)
            {
                return false;
            }

            return Equals(_token, other._token);
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
            return _token.GetHashCode();
        }

        #endregion Methods
    }
}