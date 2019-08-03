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
                return GetDescription(valueEnum);
            }
            else if (value is VideoQuality[] valueEnumArray)
            {
                string[] resultArray = new string[valueEnumArray.Length];
                for(int index = 0;index< valueEnumArray.Length;index++)
                    resultArray[index] = GetDescription(valueEnumArray[index]);
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
                if (GetDescription(enumVal).Equals(value))
                    return enumVal;
            return VideoQuality.Source;
            //throw new NotSupportedException("This converter can only convert!");
        }

        private static string GetDescription(Enum value)
        {
            Type type = value.GetType();
            string Name = Enum.GetName(type, value);
            if (Name != null)
            {
                System.Reflection.FieldInfo field = type.GetField(Name);
                if (field != null)
                {
                    if (Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DescriptionAttribute)) is System.ComponentModel.DescriptionAttribute attr)
                    {
                        return attr.Description;
                    }
                }
            }
            return value.ToString();
        }

        #endregion IValueConverter Members
    }
}