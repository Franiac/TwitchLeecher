namespace TwitchLeecher.Core.Enums
{
    public enum DownloadState
    {
        Queued,
        Paused,
        Downloading,
        Waiting,
        Concatenation,
        Canceled,
        Error,
        Done
    }
}