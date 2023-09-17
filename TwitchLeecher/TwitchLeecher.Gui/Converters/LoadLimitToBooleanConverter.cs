using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Gui.Converters
{
    public class LoadLimitToBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is LoadLimitType))
            {
                throw new ApplicationException("Value has to be of type '" + typeof(LoadLimitType).FullName + "'!");
            }

            if (!(parameter is LoadLimitType))
            {
                throw new ApplicationException("Parameter has to be of type '" + typeof(LoadLimitType).FullName + "'!");
            }

            LoadLimitType valueEnum = (LoadLimitType)value;
            LoadLimitType parameterEnum = (LoadLimitType)parameter;

            return valueEnum.Equals(parameterEnum);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
            {
                throw new ApplicationException("Value has to be of type '" + typeof(bool).FullName + "'!");
            }

            if (!(parameter is LoadLimitType))
            {
                throw new ApplicationException("Parameter has to be of type '" + typeof(LoadLimitType).FullName + "'!");
            }

            return parameter;
        }

        #endregion IValueConverter Members
    }
}