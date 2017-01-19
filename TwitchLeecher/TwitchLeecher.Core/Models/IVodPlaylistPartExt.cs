namespace TwitchLeecher.Core.Models
{
    public interface IVodPlaylistPartExt : IVodPlaylistPart
    {
        string DownloadUrl { get; }

        string LocalFile { get; }

        double Length { get; }
    }
}