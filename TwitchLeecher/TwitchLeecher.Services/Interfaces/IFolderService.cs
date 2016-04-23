namespace TwitchLeecher.Services.Interfaces
{
    public interface IFolderService
    {
        string GetAppDataFolder();

        string GetDownloadFolder();
    }
}