using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Gui.Converters
{
    public class DownloadStatusToColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DownloadStatus))
            {
                throw new ApplicationException("Value has to be of type '" + typeof(DownloadStatus).FullName + "'!");
            }

            DownloadStatus valueEnum = (DownloadStatus)value;

            switch (valueEnum)
            {
                case DownloadStatus.Queued:
                    return (Color)ColorConverter.ConvertFromString("#FFFFD400");

                case DownloadStatus.Error:
                    return (Color)ColorConverter.ConvertFromString("#FFFF1900");

                case DownloadStatus.Canceled:
                    return (Color)ColorConverter.ConvertFromString("#FFFF1900");

                default:
                    return (Color)ColorConverter.ConvertFromString("#FF03C600");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This converter can only convert!");
        }

        #endregion IValueConverter Members
    }
}