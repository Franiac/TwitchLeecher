namespace TwitchLeecher.Gui.Controls
{
    using System.Windows;
    using Views;

    public static class ThemedMessageBox
    {
        public static MessageBoxResult Show(string messageBoxText)
        {
            MessageBoxWindow msg = new MessageBoxWindow(messageBoxText);
            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            MessageBoxWindow msg = new MessageBoxWindow(messageBoxText, caption);
            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText)
        {
            MessageBoxWindow msg = new MessageBoxWindow(messageBoxText)
            {
                Owner = owner
            };

            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption)
        {
            MessageBoxWindow msg = new MessageBoxWindow(messageBoxText, caption)
            {
                Owner = owner
            };

            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            MessageBoxWindow msg = new MessageBoxWindow(messageBoxText, caption, button);
            msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            MessageBoxWindow msg = new MessageBoxWindow(messageBoxText, caption, button, icon);
            msg.ShowDialog();

            return msg.Result;
        }
    }
}