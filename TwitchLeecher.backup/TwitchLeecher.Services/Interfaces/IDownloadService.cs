using System.Collections.ObjectModel;
using System.ComponentModel;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IDownloadService : INotifyPropertyChanged
    {
        #region Properties

        ObservableCollection<TwitchVideoDownload> Downloads { get; }

        #endregion Properties

        #region Methods

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