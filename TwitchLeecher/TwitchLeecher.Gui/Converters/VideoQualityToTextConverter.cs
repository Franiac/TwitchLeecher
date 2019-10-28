using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Gui.Converters
{
    public class VideoQualityToTextConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VideoQuality valueEnum)
            {
                return valueEnum.GetDescription();
            }
            else if (value is VideoQuality[] valueEnumArray)
            {
                string[] resultArray = new string[valueEnumArray.Length];
                for(int index = 0;index< valueEnumArray.Length;index++)
                    resultArray[index] = valueEnumArray[index].GetDescription();
                return resultArray;
            }
            else
            { 
                throw new ApplicationException("Value has to be of type '" + typeof(VideoQuality).FullName + "'!");
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            VideoQuality[] allValues = (VideoQuality[])Enum.GetValues(typeof(VideoQuality));
            foreach (VideoQuality enumVal in allValues)
                if (enumVal.GetDescription().Equals(value))
                    return enumVal;
            return VideoQuality.Source;
            //throw new NotSupportedException("This converter can only convert!");
        }
        #endregion IValueConverter Members
    }
}