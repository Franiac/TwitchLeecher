using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Gui.Converters
{
    public class DownloadStateToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DownloadState))
            {
                throw new ApplicationException("Value has to be of type '" + typeof(DownloadState).FullName + "'!");
            }

            if (!(parameter is DownloadState))
            {
                throw new ApplicationException("Parameter has to be of type '" + typeof(DownloadState).FullName + "'!");
            }

            DownloadState valueEnum = (DownloadState)value;
            DownloadState parameterEnum = (DownloadState)parameter;

            return valueEnum.Equals(parameterEnum) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This converter can only convert!");
        }

        #endregion IValueConverter Members
    }
}