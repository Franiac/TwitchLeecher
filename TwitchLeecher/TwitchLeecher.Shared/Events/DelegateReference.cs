using System;
using System.Reflection;

namespace TwitchLeecher.Shared.Events
{
    public class DelegateReference : IDelegateReference
    {
        #region Fields

        private readonly Delegate @delegate;
        private readonly Type delegateType;
        private readonly MethodInfo method;
        private readonly WeakReference weakReference;

        #endregion Fields

        #region Constructors

        public DelegateReference(Delegate @delegate, bool keepReferenceAlive)
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

            if (keepReferenceAlive)
            {
                this.@delegate = @delegate;
            }
            else
            {
                weakReference = new WeakReference(@delegate.Target);
                method = @delegate.GetMethodInfo();
                delegateType = @delegate.GetType();
            }
        }

        #endregion Constructors

        #region Properties

        public Delegate Target
        {
            get
            {
                if (@delegate != null)
                {
                    return @delegate;
                }
                else
                {
                    return TryGetDelegate();
                }
            }
        }

        #endregion Properties

        #region Methods

        private Delegate TryGetDelegate()
        {
            if (method.IsStatic)
            {
                return method.CreateDelegate(delegateType, null);
            }

            object target = weakReference.Target;

            if (target != null)
            {
                return method.CreateDelegate(delegateType, target);
            }
            return null;
        }

        #endregion Methods
    }
}