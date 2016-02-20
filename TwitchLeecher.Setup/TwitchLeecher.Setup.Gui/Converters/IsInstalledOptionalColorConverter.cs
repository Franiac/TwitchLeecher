using System;
using System.Globalization;
using System.Windows.Data;

namespace TwitchLeecher.Setup.Gui.Converters
{
    public class IsInstalledOptionalColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "#FF00C800";
            }
            else
            {
                return "#FFFF9600";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}