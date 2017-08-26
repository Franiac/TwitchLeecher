using System;
using System.Globalization;
using System.Windows.Data;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Gui.Converters
{
    public class LoadLimitToBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is LoadLimit))
            {
                throw new ApplicationException("Value has to be of type '" + typeof(LoadLimit).FullName + "'!");
            }

            if (!(parameter is LoadLimit))
            {
                throw new ApplicationException("Parameter has to be of type '" + typeof(LoadLimit).FullName + "'!");
            }

            LoadLimit valueEnum = (LoadLimit)value;
            LoadLimit parameterEnum = (LoadLimit)parameter;

            return valueEnum.Equals(parameterEnum);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
            {
                throw new ApplicationException("Value has to be of type '" + typeof(bool).FullName + "'!");
            }

            if (!(parameter is LoadLimit))
            {
                throw new ApplicationException("Parameter has to be of type '" + typeof(LoadLimit).FullName + "'!");
            }

            return parameter;
        }

        #endregion IValueConverter Members
    }
}