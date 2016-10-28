using System.Collections.ObjectModel;
using System.ComponentModel;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface ITwitchService : INotifyPropertyChanged
    {
        #region Properties

        bool IsAuthorized { get; }

        ObservableCollection<TwitchVideo> Videos { get; }

        ObservableCollection<TwitchVideoDownload> Downloads { get; }

        #endregion Properties

        #region Methods

        VodAuthInfo RetrieveVodAuthInfo(string idTrimmed);

        bool UserExists(string username);

        bool Authorize(string accessToken);

        void RevokeAuthorization();

        void Search(SearchParameters searchParams);

        void Enqueue(DownloadParameters downloadParams);

        void Cancel(string id);

        void Retry(string id);

        void Remove(string id);

        void Pause();

        void Resume();

        bool CanShutdown();

        void Shutdown();

        bool IsFileNameUsed(string fullPath);

        #endregion Methods
    }
}