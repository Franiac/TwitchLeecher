using System;
using System.Globalization;
using Avalonia.Data.Converters;
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

            return valueEnum.Equals(parameterEnum);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This converter can only convert!");
        }

        #endregion IValueConverter Members
    }
}