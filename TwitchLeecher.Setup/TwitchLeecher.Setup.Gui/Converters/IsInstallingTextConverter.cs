using System;
using System.Globalization;
using System.Windows.Data;

namespace TwitchLeecher.Setup.Gui.Converters
{
    public class IsInstallingTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return "Installing...";
            }
            else
            {
                return "Install";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}