using System;
using System.Globalization;
using System.Windows.Data;

namespace TwitchLeecher.Setup.Gui.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Invert((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Invert((bool)value);
        }

        private bool Invert(bool value)
        {
            if (value)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}