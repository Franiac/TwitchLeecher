using System;
using System.Windows.Input;

namespace TwitchLeecher.Core.Models
{
    public class MenuCommand
    {
        #region Constructors

        public MenuCommand(ICommand command, string label, string icon, int width = 120)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentNullException(nameof(label));
            }

            if (string.IsNullOrWhiteSpace(icon))
            {
                throw new ArgumentNullException(nameof(icon));
            }

            Command = command ?? throw new ArgumentNullException(nameof(command));
            Label = label;
            Icon = icon;
            Width = width;
        }

        #endregion Constructors

        #region Properties

        public ICommand Command { get; private set; }

        public string Label { get; private set; }

        public string Icon { get; private set; }

        public double Width { get; private set; }

        #endregion Properties
    }
}