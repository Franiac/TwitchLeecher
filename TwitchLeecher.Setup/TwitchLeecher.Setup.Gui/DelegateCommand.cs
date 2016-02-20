using System;
using System.Windows.Input;

namespace TwitchLeecher.Setup.Gui.Command
{
    internal class DelegateCommand : ICommand
    {
        #region Fields

        private readonly Action action;
        private readonly Func<bool> canExecute;

        private readonly Action<object> actionPar;
        private readonly Func<object, bool> canExecutePar;

        #endregion Fields

        #region Constructors

        public DelegateCommand(Action action)
            : this(action, null)
        {
        }

        public DelegateCommand(Action action, Func<bool> canExecute)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this.action = action;
            this.canExecute = canExecute;
        }

        public DelegateCommand(Action<object> action)
            : this(action, null)
        {
        }

        public DelegateCommand(Action<object> actionPar, Func<object, bool> canExecute)
        {
            if (actionPar == null)
            {
                throw new ArgumentNullException("actionPar");
            }

            this.actionPar = actionPar;
            this.canExecutePar = canExecute;
        }

        #endregion Constructors

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (this.canExecutePar == null)
            {
                return this.canExecute == null ? true : this.canExecute();
            }
            else
            {
                return this.canExecutePar == null ? true : this.canExecutePar(parameter);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (this.canExecute != null || this.canExecutePar != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (this.canExecute != null || canExecutePar != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public void Execute(object parameter)
        {
            if (this.action != null)
            {
                this.action();
            }
            else
            {
                this.actionPar(parameter);
            }
        }

        #endregion ICommand Members
    }
}