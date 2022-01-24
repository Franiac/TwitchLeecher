using System.Collections.ObjectModel;
using System.ComponentModel;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Gui.Interfaces
{
    public interface ISearchService : INotifyPropertyChanged
    {
        ObservableCollection<TwitchVideo> Videos { get; }

        SearchParameters LastSearchParams { get; }

        void PerformSearch(SearchParameters searchParams);
    }
}