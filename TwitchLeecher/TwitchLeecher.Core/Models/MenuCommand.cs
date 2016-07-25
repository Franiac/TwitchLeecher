using System;
using System.Windows.Input;

namespace TwitchLeecher.Core.Models
{
    public class MenuCommand
    {
        #region Constructors

        public MenuCommand(ICommand command, string label, string icon)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentNullException(nameof(label));
            }

            if (string.IsNullOrWhiteSpace(icon))
            {
                throw new ArgumentNullException(nameof(icon));
            }

            this.Command = command;
            this.Label = label;
            this.Icon = icon;
        }

        #endregion Constructors

        #region Properties

        public ICommand Command { get; private set; }

        public string Label { get; private set; }

        public string Icon { get; private set; }

        #endregion Properties
    }
}