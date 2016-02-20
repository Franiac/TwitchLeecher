using System;
using System.Globalization;
using System.Windows.Data;

namespace TwitchLeecher.Setup.Gui.Converters
{
    public class IsInstalledTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "Installed";
            }
            else
            {
                return "Not Installed";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}