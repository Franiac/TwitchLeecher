using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TwitchLeecher.Shared.Commands
{
    public abstract class DelegateCommandBase : ICommand
    {
        private bool _isActive;

        private readonly HashSet<string> propertiesToObserve = new HashSet<string>();
        private INotifyPropertyChanged inpc;

        protected readonly Func<object, Task> executeMethod;

        protected Func<object, bool> canExecuteMethod;

        protected DelegateCommandBase(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod), "Neither the executeMethod nor the canExecuteMethod delegates can be null");

            this.executeMethod = (arg) => { executeMethod(arg); return Task.Delay(0); };
            this.canExecuteMethod = canExecuteMethod;
        }

        protected DelegateCommandBase(Func<object, Task> executeMethod, Func<object, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod), "Neither the executeMethod nor the canExecuteMethod delegates can be null");

            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        public virtual event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        async void ICommand.Execute(object parameter)
        {
            await Execute(parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter);
        }

        protected virtual async Task Execute(object parameter)
        {
            await executeMethod(parameter);
        }

        protected virtual bool CanExecute(object parameter)
        {
            return canExecuteMethod(parameter);
        }

        protected internal void ObservesPropertyInternal<T>(Expression<Func<T>> propertyExpression)
        {
            AddPropertyToObserve(PropertySupport.ExtractPropertyName(propertyExpression));
            HookInpc(propertyExpression.Body as MemberExpression);
        }

        protected internal void ObservesCanExecuteInternal(Expression<Func<object, bool>> canExecuteExpression)
        {
            canExecuteMethod = canExecuteExpression.Compile();
            AddPropertyToObserve(PropertySupport.ExtractPropertyNameFromLambda(canExecuteExpression));
            HookInpc(canExecuteExpression.Body as MemberExpression);
        }

        protected void HookInpc(MemberExpression expression)
        {
            if (expression == null)
                return;

            if (inpc == null)
            {
                var constantExpression = expression.Expression as ConstantExpression;
                if (constantExpression != null)
                {
                    inpc = constantExpression.Value as INotifyPropertyChanged;
                    if (inpc != null)
                        inpc.PropertyChanged += Inpc_PropertyChanged;
                }
            }
        }

        protected void AddPropertyToObserve(string property)
        {
            if (propertiesToObserve.Contains(property))
                throw new ArgumentException(String.Format("{0} is already being observed.", property));

            propertiesToObserve.Add(property);
        }

        private void Inpc_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (propertiesToObserve.Contains(e.PropertyName))
                RaiseCanExecuteChanged();
        }
    }
}