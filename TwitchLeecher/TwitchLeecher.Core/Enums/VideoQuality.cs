namespace TwitchLeecher.Core.Enums
{
    public enum VideoQuality
    {
        [System.ComponentModel.Description("Source")] Source,
        [System.ComponentModel.Description("1440p60")] q1440f60,
        [System.ComponentModel.Description("1440p30")] q1440f30,
        [System.ComponentModel.Description("1080p60")] q1080f60,
        [System.ComponentModel.Description("1080p30")] q1080f30,
        [System.ComponentModel.Description("900p60")] q900f60,
        [System.ComponentModel.Description("900p30")] q900f30,
        [System.ComponentModel.Description("720p60")] q720f60,
        [System.ComponentModel.Description("720p30")] q720f30,
        //[System.ComponentModel.Description("480p60")] q480f60,
        [System.ComponentModel.Description("480p30")] q480f30,
        //[System.ComponentModel.Description("360p60")] q360f60,
        [System.ComponentModel.Description("360p30")] q360f30,
        //[System.ComponentModel.Description("160p60")] q160p60,
        [System.ComponentModel.Description("160p30")] q160f30,
        [System.ComponentModel.Description("Audio only")] AudioOnly
    }
}