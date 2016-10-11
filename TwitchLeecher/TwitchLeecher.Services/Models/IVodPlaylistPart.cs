namespace TwitchLeecher.Services.Models
{
    internal interface IVodPlaylistPart
    {
        int Index { get; }

        string GetOutput();
    }
}