using TwitchLeecher.Core.Attributes;

namespace TwitchLeecher.Core.Enums
{
    public enum VideoQuality
    {
        [EnumDisplayName("Source")]
        Source,

        [EnumDisplayName("High")]
        High,

        [EnumDisplayName("Medium")]
        Medium,

        [EnumDisplayName("Low")]
        Low,

        [EnumDisplayName("Mobile")]
        Mobile,

        [EnumDisplayName("Audio Only")]
        AudioOnly
    }
}