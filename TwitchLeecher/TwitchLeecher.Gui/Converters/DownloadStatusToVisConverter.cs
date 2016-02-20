using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Gui.Converters
{
    public class DownloadStatusToVisConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DownloadStatus))
            {
                throw new ApplicationException("Value has to be of type '" + typeof(DownloadStatus).FullName + "'!");
            }

            if (!(parameter is DownloadStatus))
            {
                throw new ApplicationException("Parameter has to be of type '" + typeof(DownloadStatus).FullName + "'!");
            }

            DownloadStatus valueEnum = (DownloadStatus)value;
            DownloadStatus parameterEnum = (DownloadStatus)parameter;

            return valueEnum.Equals(parameterEnum) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This converter can only convert!");
        }

        #endregion IValueConverter Members
    }
}