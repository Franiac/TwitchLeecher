using System.Collections.ObjectModel;
using System.ComponentModel;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface ITwitchService : INotifyPropertyChanged
    {
        #region Properties

        ObservableCollection<TwitchVideo> Videos { get; }

        ObservableCollection<TwitchVideoDownload> Downloads { get; }

        #endregion Properties

        #region Methods

        bool UserExists(string username);

        void Search(SearchParameters searchParams);

        void Enqueue(DownloadParameters downloadParams);

        void Cancel(string id);

        void Retry(string id);

        void Remove(string id);

        void Pause();

        void Resume();

        bool CanShutdown();

        void Shutdown();

        #endregion Methods
    }
}