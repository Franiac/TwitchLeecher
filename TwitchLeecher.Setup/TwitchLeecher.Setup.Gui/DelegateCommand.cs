using System;
using System.Windows.Input;

namespace TwitchLeecher.Setup.Gui.Command
{
    internal class DelegateCommand : ICommand
    {
        #region Fields

        private readonly Action _action;
        private readonly Func<bool> _canExecute;

        private readonly Action<object> _actionPar;
        private readonly Func<object, bool> _canExecutePar;

        #endregion Fields

        #region Constructors

        public DelegateCommand(Action action)
            : this(action, null)
        {
        }

        public DelegateCommand(Action action, Func<bool> canExecute)
        {
            _action = action ?? throw new ArgumentNullException("action");
            _canExecute = canExecute;
        }

        public DelegateCommand(Action<object> action)
            : this(action, null)
        {
        }

        public DelegateCommand(Action<object> actionPar, Func<object, bool> canExecute)
        {
            _actionPar = actionPar ?? throw new ArgumentNullException("actionPar");
            _canExecutePar = canExecute;
        }

        #endregion Constructors

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (_canExecutePar == null)
            {
                return _canExecute == null || _canExecute();
            }
            else
            {
                return _canExecutePar == null || _canExecutePar(parameter);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null || _canExecutePar != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (_canExecute != null || _canExecutePar != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public void Execute(object parameter)
        {
            if (_action != null)
            {
                _action();
            }
            else
            {
                _actionPar(parameter);
            }
        }

        #endregion ICommand Members
    }
}