﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TwitchLeecher.Core.Enums;

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

            switch (valueEnum)
            {
                case DownloadState.Queued:
                    return (Color)ColorConverter.ConvertFromString("#FFFFD400");

                case DownloadState.Error:
                    return (Color)ColorConverter.ConvertFromString("#FFFF1900");

                case DownloadState.Canceled:
                    return (Color)ColorConverter.ConvertFromString("#FFFF1900");

                case DownloadState.Waiting:
                    return (Color)ColorConverter.ConvertFromString("#FFE1CD00");
                    
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