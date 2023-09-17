using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Gui.Converters
{
    public class SearchTypeToBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SearchType))
            {
                throw new ApplicationException("Value has to be of type '" + typeof(SearchType).FullName + "'!");
            }

            if (!(parameter is SearchType))
            {
                throw new ApplicationException("Parameter has to be of type '" + typeof(SearchType).FullName + "'!");
            }

            SearchType valueEnum = (SearchType)value;
            SearchType parameterEnum = (SearchType)parameter;

            return valueEnum.Equals(parameterEnum);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
            {
                throw new ApplicationException("Value has to be of type '" + typeof(bool).FullName + "'!");
            }

            if (!(parameter is SearchType))
            {
                throw new ApplicationException("Parameter has to be of type '" + typeof(SearchType).FullName + "'!");
            }

            return parameter;
        }

        #endregion IValueConverter Members
    }
}