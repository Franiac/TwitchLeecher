namespace TwitchLeecher.Core.Enums
{
    public enum DownloadState
    {
        Queued,
        Paused,
        Downloading,
        WaitConcatenation,
        Concatenation,
        Converting,
        Canceled,
        Error,
        Done
    }
}