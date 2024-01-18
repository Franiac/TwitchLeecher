using System;
using System.Drawing;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using TwitchLeecher.Core.Enums;
using Brush = Avalonia.Media.Brush;
using Color = System.Drawing.Color;

namespace TwitchLeecher.Gui.Converters
{
    public class DownloadStateToColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DownloadState))
            {
                throw new ApplicationException("Value has to be of type '" + typeof(DownloadState).FullName + "'!");
            }

            DownloadState valueEnum = (DownloadState)value;
            var converter = new BrushConverter();

            switch (valueEnum)
            {
                case DownloadState.Queued:
                case DownloadState.CompletedWithWarning:
                    return Brush.Parse("#FFFFD400");

                case DownloadState.Error:
                    return Brush.Parse("#FFFF1900");

                case DownloadState.Canceled:
                    return Brush.Parse("#FFFF1900");

                default:
                    return Brush.Parse("#FF03C600");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This converter can only convert!");
        }

        #endregion IValueConverter Members
    }
}