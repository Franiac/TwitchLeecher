using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace TwitchLeecher.Shared.Commands
{
    public class DelegateCommand<T> : DelegateCommandBase
    {
        public DelegateCommand(Action<T> executeMethod)
               : this(executeMethod, (o) => true)
        {
        }

        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
            : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o))
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod), "Neither the executeMethod nor the canExecuteMethod delegates can be null");

            TypeInfo genericTypeInfo = typeof(T).GetTypeInfo();

            if (genericTypeInfo.IsValueType)
            {
                if ((!genericTypeInfo.IsGenericType) || (!typeof(Nullable<>).GetTypeInfo().IsAssignableFrom(genericTypeInfo.GetGenericTypeDefinition().GetTypeInfo())))
                {
                    throw new InvalidCastException("T for DelegateCommand<T> is not an object nor Nullable");
                }
            }
        }

        public DelegateCommand<T> ObservesProperty<TP>(Expression<Func<TP>> propertyExpression)
        {
            ObservesPropertyInternal(propertyExpression);
            return this;
        }

        public DelegateCommand<T> ObservesCanExecute(Expression<Func<object, bool>> canExecuteExpression)
        {
            ObservesCanExecuteInternal(canExecuteExpression);
            return this;
        }

        public static DelegateCommand<T> FromAsyncHandler(Func<T, Task> executeMethod)
        {
            return new DelegateCommand<T>(executeMethod);
        }

        public static DelegateCommand<T> FromAsyncHandler(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
        {
            return new DelegateCommand<T>(executeMethod, canExecuteMethod);
        }

        public virtual bool CanExecute(T parameter)
        {
            return base.CanExecute(parameter);
        }

        public virtual Task Execute(T parameter)
        {
            return base.Execute(parameter);
        }

        protected DelegateCommand(Func<T, Task> executeMethod)
            : this(executeMethod, (o) => true)
        {
        }

        protected DelegateCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
            : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o))
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod), "Neither the executeMethod nor the canExecuteMethod delegates can be null");
        }
    }

    public class DelegateCommand : DelegateCommandBase
    {
        public DelegateCommand(Action executeMethod)
             : this(executeMethod, () => true)
        {
        }

        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
             : base((o) => executeMethod(), (o) => canExecuteMethod())
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod), "Neither the executeMethod nor the canExecuteMethod delegates can be null");
        }

        public DelegateCommand ObservesProperty<T>(Expression<Func<T>> propertyExpression)
        {
            ObservesPropertyInternal(propertyExpression);
            return this;
        }

        public DelegateCommand ObservesCanExecute(Expression<Func<object, bool>> canExecuteExpression)
        {
            ObservesCanExecuteInternal(canExecuteExpression);
            return this;
        }

        public static DelegateCommand FromAsyncHandler(Func<Task> executeMethod)
        {
            return new DelegateCommand(executeMethod);
        }

        public static DelegateCommand FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod)
        {
            return new DelegateCommand(executeMethod, canExecuteMethod);
        }

        public virtual Task Execute()
        {
            return Execute(null);
        }

        public virtual bool CanExecute()
        {
            return CanExecute(null);
        }

        protected DelegateCommand(Func<Task> executeMethod)
            : this(executeMethod, () => true)
        {
        }

        protected DelegateCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
            : base((o) => executeMethod(), (o) => canExecuteMethod())
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod), "Neither the executeMethod nor the canExecuteMethod delegates can be null");
        }
    }
}