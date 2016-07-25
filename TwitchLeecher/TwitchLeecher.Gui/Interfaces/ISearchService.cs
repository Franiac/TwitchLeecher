using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Gui.Interfaces
{
    public interface ISearchService
    {
        SearchParameters LastSearchParams { get; }

        void PerformSearch(SearchParameters searchParams);
    }
}