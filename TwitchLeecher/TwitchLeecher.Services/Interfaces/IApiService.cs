using System.Collections.ObjectModel;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IApiService
    {
        TwitchAuthInfo ValidateAuthentication(string accessToken);

        void RevokeAuthentication(string accessToken);

        VodAuthInfo RetrieveVodAuthInfo(string id);
        
        bool ChannelExists(string channel);

        string GetChannelIdByName(string channel);

        ObservableCollection<TwitchVideo> Search(SearchParameters searchParams);
    }
}