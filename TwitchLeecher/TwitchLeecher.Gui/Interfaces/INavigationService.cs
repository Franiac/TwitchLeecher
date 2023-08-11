using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Gui.Interfaces
{
    public interface INavigationService
    {
        void ShowAuth();

        void ShowWelcome();

        void ShowLoading();

        void ShowSearch();

        void ShowSearchResults();

        void ShowDownload(DownloadParameters downloadParams);

        void ShowDownloads();

        void ShowPreferences();

        void ShowInfo();

        void ShowLog(TwitchVideoDownload download);

        void NavigateBack();
        void ShowAuthSubOnly();
    }
}