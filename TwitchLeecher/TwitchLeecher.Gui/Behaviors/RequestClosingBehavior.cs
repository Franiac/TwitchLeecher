using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Avalonia.Controls;

namespace TwitchLeecher.Gui.Behaviors
{
    public class RequestClosingBehavior
    {
        #region RequestClosing

        public static readonly DependencyProperty RequestClosingProperty = DependencyProperty.RegisterAttached(
            "RequestClosing", typeof(ICommand), typeof(RequestClosingBehavior),
            new UIPropertyMetadata(new PropertyChangedCallback(RequestClosingChanged)));

        public static ICommand GetRequestClosing(Window? obj)
        {
            return (ICommand)obj.GetValue(RequestClosingProperty);
        }

        public static void SetRequestClosing(DependencyObject obj, ICommand value)
        {
            obj.SetValue(RequestClosingProperty, value);
        }

        private static void RequestClosingChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (target is Window window)
            {
                if (e.NewValue != null)
                {
                    window.Closing += Window_Closing;
                }
                else
                {
                    window.Closing -= Window_Closing;
                }
            }
        }

        #endregion RequestClosing

        #region EventHandler

        private static void Window_Closing(object sender, CancelEventArgs e)
        {
            ICommand request = GetRequestClosing(sender as Window);

            if (request != null)
            {
                if (!request.CanExecute(null))
                {
                    e.Cancel = true;
                }
            }
        }

        #endregion EventHandler
    }
}