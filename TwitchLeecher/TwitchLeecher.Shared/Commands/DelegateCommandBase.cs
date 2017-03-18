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
        #region Fields

        private readonly HashSet<string> _propertiesToObserve;
        private INotifyPropertyChanged _inpc;

        protected readonly Func<object, Task> _executeMethod;
        protected Func<object, bool> _canExecuteMethod;

        #endregion Fields

        #region Constructors

        protected DelegateCommandBase(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException(nameof(executeMethod), "Neither the executeMethod nor the canExecuteMethod delegates can be null");
            }

            _propertiesToObserve = new HashSet<string>();
            _executeMethod = (arg) => { executeMethod(arg); return Task.Delay(0); };
            _canExecuteMethod = canExecuteMethod;
        }

        protected DelegateCommandBase(Func<object, Task> executeMethod, Func<object, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
            {
                throw new ArgumentNullException(nameof(executeMethod), "Neither the executeMethod nor the canExecuteMethod delegates can be null");
            }
            _propertiesToObserve = new HashSet<string>();
            _executeMethod = executeMethod;
            _canExecuteMethod = canExecuteMethod;
        }

        #endregion Constructors

        #region Methods

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
            await _executeMethod(parameter);
        }

        protected virtual bool CanExecute(object parameter)
        {
            return _canExecuteMethod(parameter);
        }

        protected internal void ObservesPropertyInternal<T>(Expression<Func<T>> propertyExpression)
        {
            AddPropertyToObserve(PropertySupport.ExtractPropertyName(propertyExpression));
            HookInpc(propertyExpression.Body as MemberExpression);
        }

        protected internal void ObservesCanExecuteInternal(Expression<Func<object, bool>> canExecuteExpression)
        {
            _canExecuteMethod = canExecuteExpression.Compile();
            AddPropertyToObserve(PropertySupport.ExtractPropertyNameFromLambda(canExecuteExpression));
            HookInpc(canExecuteExpression.Body as MemberExpression);
        }

        protected void HookInpc(MemberExpression expression)
        {
            if (expression == null)
                return;

            if (_inpc == null)
            {
                if (expression.Expression is ConstantExpression constantExpression)
                {
                    _inpc = constantExpression.Value as INotifyPropertyChanged;
                    if (_inpc != null)
                        _inpc.PropertyChanged += Inpc_PropertyChanged;
                }
            }
        }

        protected void AddPropertyToObserve(string property)
        {
            if (_propertiesToObserve.Contains(property))
                throw new ArgumentException(String.Format("{0} is already being observed.", property));

            _propertiesToObserve.Add(property);
        }

        private void Inpc_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_propertiesToObserve.Contains(e.PropertyName))
                FireCanExecuteChanged();
        }

        #endregion Methods

        #region Events

        public virtual event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public void FireCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        #endregion Events
    }
}