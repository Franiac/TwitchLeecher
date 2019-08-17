using System;
using System.ComponentModel;

namespace TwitchLeecher.Core.Enums
{
    public enum VideoQuality
    {
        [Description("Source"), Descriptor(fps: -1, resolutionY: -1)] Source,
        [Description("1440p60"), Descriptor(fps: 60, resolutionY: 1440)] q1440f60,
        [Description("1440p30"), Descriptor(fps: 30, resolutionY: 1440)] q1440f30,
        [Description("1080p60"), Descriptor(fps: 60, resolutionY: 1080)] q1080f60,
        [Description("1080p30"), Descriptor(fps: 30, resolutionY: 1080)] q1080f30,
        [Description("900p60"), Descriptor(fps: 60, resolutionY: 900)] q900f60,
        [Description("900p30"), Descriptor(fps: 30, resolutionY: 900)] q900f30,
        [Description("720p60"), Descriptor(fps: 60, resolutionY: 720)] q720f60,
        [Description("720p30"), Descriptor(fps: 30, resolutionY: 720)] q720f30,
        //[Description("480p60"), Descriptor(fps: 60, resolutionY: 480)] q480f60,
        [Description("480p30"), Descriptor(fps: 30, resolutionY: 480)] q480f30,
        //[Description("360p60"), Descriptor(fps: 60, resolutionY: 360)] q360f60,
        [Description("360p30"), Descriptor(fps: 30, resolutionY: 360)] q360f30,
        //[Description("160p60"), Descriptor(fps: 60, resolutionY: 160)] q160p60,
        [Description("160p30"), Descriptor(fps: 30, resolutionY: 160)] q160f30,
        [Description("Audio only"), Descriptor(fps: 0, resolutionY: 0)] AudioOnly
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DescriptorAttribute : Attribute
    {
        public int FPS { get; }
        public int ResolutionY { get; }

        public DescriptorAttribute(int fps, int resolutionY)
        {
            FPS = fps;
            ResolutionY = resolutionY;
        }
    }

    public static partial class GlobalMethods
    {
        public static int GetFps(this VideoQuality item)
        {
            Type type = item.GetType();
            string itemName = Enum.GetName(type, item);
            if (itemName != null)
            {
                System.Reflection.FieldInfo itemField = type.GetField(itemName);
                if (itemField != null)
                {
                    if (Attribute.GetCustomAttribute(itemField, typeof(DescriptorAttribute)) is DescriptorAttribute attr)
                    {
                        return attr.FPS;
                    }
                }
            }
            return -1;
        }

        public static int GetResolutionY(this VideoQuality item)
        {
            Type type = item.GetType();
            string itemName = Enum.GetName(type, item);
            if (itemName != null)
            {
                System.Reflection.FieldInfo itemField = type.GetField(itemName);
                if (itemField != null)
                {
                    if (Attribute.GetCustomAttribute(itemField, typeof(DescriptorAttribute)) is DescriptorAttribute attr)
                    {
                        return attr.ResolutionY;
                    }
                }
            }
            return -1;
        }

        public static string GetDescription(this VideoQuality value)
        {
            Type type = value.GetType();
            string Name = Enum.GetName(type, value);
            if (Name != null)
            {
                System.Reflection.FieldInfo field = type.GetField(Name);
                if (field != null)
                {
                    if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                    {
                        return attr.Description;
                    }
                }
            }
            return value.ToString();
        }

    }
}