using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace TwitchLeecher.Shared.Commands
{
    public class CompositeCommand : ICommand
    {
        private readonly List<ICommand> registeredCommands = new List<ICommand>();
        private readonly EventHandler onRegisteredCommandCanExecuteChangedHandler;

        public CompositeCommand()
        {
            this.onRegisteredCommandCanExecuteChangedHandler = new EventHandler(this.OnRegisteredCommandCanExecuteChanged);
        }

        public virtual void RegisterCommand(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (command == this)
            {
                throw new ArgumentException("Cannot register a CompositeCommand in itself");
            }

            lock (this.registeredCommands)
            {
                if (this.registeredCommands.Contains(command))
                {
                    throw new InvalidOperationException("Cannot register the same command twice in the same CompositeCommand");
                }
                this.registeredCommands.Add(command);
            }

            command.CanExecuteChanged += this.onRegisteredCommandCanExecuteChangedHandler;
            this.OnCanExecuteChanged();
        }

        public virtual void UnregisterCommand(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            bool removed;
            lock (this.registeredCommands)
            {
                removed = this.registeredCommands.Remove(command);
            }

            if (removed)
            {
                command.CanExecuteChanged -= this.onRegisteredCommandCanExecuteChangedHandler;
                this.OnCanExecuteChanged();
            }
        }

        private void OnRegisteredCommandCanExecuteChanged(object sender, EventArgs e)
        {
            this.OnCanExecuteChanged();
        }

        public virtual bool CanExecute(object parameter)
        {
            bool hasEnabledCommandsThatShouldBeExecuted = false;

            ICommand[] commandList;
            lock (this.registeredCommands)
            {
                commandList = this.registeredCommands.ToArray();
            }
            foreach (ICommand command in commandList)
            {
                if (this.ShouldExecute(command))
                {
                    if (!command.CanExecute(parameter))
                    {
                        return false;
                    }

                    hasEnabledCommandsThatShouldBeExecuted = true;
                }
            }

            return hasEnabledCommandsThatShouldBeExecuted;
        }

        public virtual event EventHandler CanExecuteChanged;

        public virtual void Execute(object parameter)
        {
            Queue<ICommand> commands;
            lock (this.registeredCommands)
            {
                commands = new Queue<ICommand>(this.registeredCommands.Where(this.ShouldExecute).ToList());
            }

            while (commands.Count > 0)
            {
                ICommand command = commands.Dequeue();
                command.Execute(parameter);
            }
        }

        protected virtual bool ShouldExecute(ICommand command)
        {
            return true;
        }

        public IList<ICommand> RegisteredCommands
        {
            get
            {
                IList<ICommand> commandList;
                lock (this.registeredCommands)
                {
                    commandList = this.registeredCommands.ToList();
                }

                return commandList;
            }
        }

        protected virtual void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void Command_IsActiveChanged(object sender, EventArgs e)
        {
            this.OnCanExecuteChanged();
        }
    }
}