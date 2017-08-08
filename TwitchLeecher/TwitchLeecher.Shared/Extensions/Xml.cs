using System;
using System.Globalization;
using System.Xml.Linq;

namespace TwitchLeecher.Shared.Extensions
{
    public static class XmlExtensions
    {
        #region Constants

        public static readonly CultureInfo XML_CULTURE = CultureInfo.InvariantCulture;

        #endregion Constants

        #region Checks

        public static void CheckNotNull(this XElement xel)
        {
            if (xel == null)
            {
                throw new ApplicationException("Element is null!");
            }
        }

        public static void CheckNotNull(this XAttribute xat)
        {
            if (xat == null)
            {
                throw new ApplicationException("Attribute is null!");
            }
        }

        public static void CheckName(this XElement xel, string expectedName)
        {
            CheckNotNull(xel);

            if (string.IsNullOrWhiteSpace(expectedName) && xel.Name.LocalName != expectedName)
            {
                throw new ApplicationException("Unexpected element name '" + xel.Name.LocalName + "'! Expected name is '" + expectedName + "'.");
            }
        }

        public static void CheckValueNotNull(this XElement xel)
        {
            xel.CheckNotNull();

            if (xel.IsEmpty || xel.Value == null)
            {
                throw new ApplicationException("Incomplete element '" + xel.Name.LocalName + "': Value is null!");
            }
        }

        public static void CheckValueNotNullOrWhitespace(this XAttribute xat)
        {
            xat.CheckNotNull();

            if (string.IsNullOrWhiteSpace(xat.Value))
            {
                throw new ApplicationException("Incomplete attribute '" + xat.Name.LocalName + "': Value not specified!");
            }
        }

        public static void CheckValueNotNullOrWhitespace(this XElement xel)
        {
            xel.CheckNotNull();

            if (xel.IsEmpty || string.IsNullOrWhiteSpace(xel.Value))
            {
                throw new ApplicationException("Incomplete element '" + xel.Name.LocalName + "': Value not specified!");
            }
        }

        public static bool HasAttributeWithValue(this XElement xel, string attributeName)
        {
            xel.CheckNotNull();

            XAttribute xat = xel.Attribute(attributeName);

            return xat != null && !string.IsNullOrWhiteSpace(xat.Value);
        }

        #endregion Checks

        #region XDocument

        public static string OuterXml(this XDocument doc)
        {
            if (doc == null)
                return null;

            return doc.Declaration + Environment.NewLine + doc.ToString();
        }

        #endregion XDocument

        #region XElement

        public static XDocument ToDocument(this XElement xel)
        {
            xel.CheckNotNull();

            return new XDocument(new XDeclaration("1.0", "UTF-8", null), xel);
            ;
        }

        #region XElement Value Getters

        public static Guid GetValueAsGuid(this XElement xel)
        {
            xel.CheckValueNotNullOrWhitespace();

            return new Guid(xel.Value);
        }

        public static T GetValueAsEnum<T>(this XElement xel)
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type!");
            }

            xel.CheckValueNotNullOrWhitespace();

            try
            {
                return (T)Enum.Parse(typeof(T), xel.Value);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not convert element value '" + xel.Value + "' into enum '" + typeof(T).FullName + "'!", ex);
            }
        }

        public static Uri GetValueAsAbsoluteUri(this XElement xel, bool nullAllowed = false)
        {
            xel.CheckNotNull();

            if (!nullAllowed)
            {
                xel.CheckValueNotNull();
            }

            string valueStr = xel.Value;

            if (string.IsNullOrWhiteSpace(valueStr))
            {
                return null;
            }

            if (Uri.TryCreate(valueStr, UriKind.Absolute, out Uri u))
            {
                return u;
            }
            else
            {
                throw new ApplicationException("Could not convert string '" + valueStr + "' into absolute uri!");
            }
        }

        public static DateTime GetValueAsTime(this XElement xel)
        {
            xel.CheckValueNotNullOrWhitespace();

            try
            {
                return DateTime.UtcNow.Date + TimeSpan.ParseExact(xel.Value, "hh\\:mm", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not convert element value '" + xel.Value + "' into type '" + typeof(TimeSpan).FullName + "'!", ex);
            }
        }

        public static DateTime GetValueAsDateTime(this XElement xel)
        {
            xel.CheckValueNotNullOrWhitespace();

            try
            {
                return DateTime.Parse(xel.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not convert element value '" + xel.Value + "' into type '" + typeof(DateTime).FullName + "'!", ex);
            }
        }

        public static string GetValueAsString(this XElement xel, bool nullAllowed = false)
        {
            xel.CheckNotNull();

            if (!nullAllowed)
            {
                xel.CheckValueNotNull();
            }

            // XML stores new lines as \n
            return xel.Value.Replace(Environment.NewLine, "\n")
                .Replace("\r", "\n")
                .Replace("\n", Environment.NewLine);
        }

        public static int GetValueAsInt(this XElement xel)
        {
            xel.CheckValueNotNullOrWhitespace();

            try
            {
                return int.Parse(xel.Value);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not convert element value '" + xel.Value + "' into type '" + typeof(int).FullName + "'!", ex);
            }
        }

        public static long GetValueAsLong(this XElement xel)
        {
            xel.CheckValueNotNullOrWhitespace();

            try
            {
                return long.Parse(xel.Value);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not convert element value '" + xel.Value + "' into type '" + typeof(long).FullName + "'!", ex);
            }
        }

        public static double GetValueAsDouble(this XElement xel)
        {
            xel.CheckValueNotNullOrWhitespace();

            try
            {
                return double.Parse(xel.Value);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not convert element value '" + xel.Value + "' into type '" + typeof(double).FullName + "'!", ex);
            }
        }

        public static bool GetValueAsBool(this XElement xel)
        {
            xel.CheckValueNotNullOrWhitespace();

            try
            {
                return bool.Parse(xel.Value);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not convert element value '" + xel.Value + "' into type '" + typeof(bool).FullName + "'!", ex);
            }
        }

        public static bool? GetValueAsNullableBool(this XElement xel)
        {
            return xel.GetNullableValue<bool>();
        }

        public static decimal? GetValueAsNullableDecimal(this XElement xel)
        {
            return xel.GetNullableValue<decimal>();
        }

        public static int? GetValueAsNullableInt(this XElement xel)
        {
            return xel.GetNullableValue<int>();
        }

        public static double? GetValueAsNullableDouble(this XElement xel)
        {
            return xel.GetNullableValue<double>();
        }

        public static Version GetValueAsVersion(this XElement xel)
        {
            xel.CheckNotNull();

            try
            {
                return Version.Parse(xel.Value);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not convert element value '" + xel.Value + "' into type '" + typeof(Version).FullName + "'!", ex);
            }
        }

        #endregion XElement Value Getters

        #endregion XElement

        #region XAttribute

        #region XAttribute Value Getters

        public static T GetAttributeValueAsEnum<T>(this XElement xel, string attributeName)
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type!");
            }

            xel.CheckNotNull();

            XAttribute xat = xel.Attribute(attributeName);

            xat.CheckValueNotNullOrWhitespace();

            try
            {
                return (T)Enum.Parse(typeof(T), xat.Value);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not convert attribute value '" + xat.Value + "' of attribute '" + xat.Name.LocalName + "' into enum '" + typeof(T).FullName + "'!", ex);
            }
        }

        public static Version GetAttributeValueAsVersion(this XElement xel, string attributeName)
        {
            xel.CheckNotNull();

            XAttribute xat = xel.Attribute(attributeName);

            xat.CheckValueNotNullOrWhitespace();

            try
            {
                return Version.Parse(xat.Value);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not convert attribute value '" + xat.Value + "' of attribute '" + xat.Name.LocalName + "' into type '" + typeof(Version).FullName + "'!", ex);
            }
        }

        public static DateTime GetAttributeValueAsDateTime(this XElement xel, string attributeName)
        {
            xel.CheckNotNull();

            XAttribute xat = xel.Attribute(attributeName);

            xat.CheckValueNotNullOrWhitespace();

            try
            {
                return DateTime.Parse(xat.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not convert attribute value '" + xat.Value + "' of attribute '" + xat.Name.LocalName + "' into type '" + typeof(DateTime).FullName + "'!", ex);
            }
        }

        public static string GetAttributeValueAsString(this XElement xel, string attributeName)
        {
            xel.CheckNotNull();

            XAttribute xat = xel.Attribute(attributeName);

            xat.CheckValueNotNullOrWhitespace();

            // XML stores new lines as \n
            return xat.Value.Replace(Environment.NewLine, "\n")
                .Replace("\r", "\n")
                .Replace("\n", Environment.NewLine);
        }

        public static bool GetAttributeValueAsBool(this XElement xel, string attributeName)
        {
            return xel.GetAttributeValue<bool>(attributeName);
        }

        public static long GetAttributeValueAsLong(this XElement xel, string attributeName)
        {
            return xel.GetAttributeValue<long>(attributeName);
        }

        #endregion XAttribute Value Getters

        #endregion XAttribute

        #region Private Helpers

        private static T GetAttributeValue<T>(this XElement xel, string attributeName) where T : struct, IConvertible
        {
            xel.CheckNotNull();

            XAttribute xat = xel.Attribute(attributeName);

            xat.CheckValueNotNullOrWhitespace();

            try
            {
                return (T)Convert.ChangeType(xat.Value, typeof(T), XML_CULTURE);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not convert attribute value '" + xat.Value + "' of attribute '" + attributeName + "' into type '" + typeof(T).FullName + "'!", ex);
            }
        }

        private static T? GetNullableValue<T>(this XElement xel) where T : struct, IConvertible
        {
            xel.CheckNotNull();

            if (xel.IsEmpty || string.IsNullOrWhiteSpace(xel.Value))
            {
                return null;
            }

            try
            {
                return (T)Convert.ChangeType(xel.Value, typeof(T), XML_CULTURE);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not convert element value '" + xel.Value + "' into type '" + typeof(T).FullName + "'!", ex);
            }
        }

        #endregion Private Helpers
    }
}