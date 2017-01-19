namespace TwitchLeecher.Core.Models
{
    public interface IVodPlaylistPart
    {
        int Index { get; }

        string GetOutput();
    }
}