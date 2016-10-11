namespace TwitchLeecher.Services.Models
{
    internal interface IVodPlaylistPartExt : IVodPlaylistPart
    {
        string DownloadUrl { get; }

        string LocalFile { get; }

        double Length { get; }
    }
}