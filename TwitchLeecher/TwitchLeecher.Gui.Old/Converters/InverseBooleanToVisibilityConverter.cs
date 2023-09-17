using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TwitchLeecher.Gui.Converters
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
            {
                throw new ApplicationException("Value has to be of type '" + typeof(bool).FullName + "'!");
            }

            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Visibility))
            {
                throw new ApplicationException("Value has to be of type '" + typeof(bool).FullName + "'!");
            }

            return (Visibility)value != Visibility.Visible;
        }

        #endregion IValueConverter Members
    }
}