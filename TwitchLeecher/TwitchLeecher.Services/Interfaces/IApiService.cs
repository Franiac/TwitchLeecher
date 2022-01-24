using System.Collections.Generic;
using System.Collections.ObjectModel;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IApiService
    {
        TwitchAuthInfo ValidateAuthentication(string accessToken);

        void RevokeAuthentication(string accessToken);

        TwitchVideoAuthInfo RetrieveVodAuthInfo(string id);

        Dictionary<TwitchVideoQuality, string> GetPlaylistSummaray(string vodId, TwitchVideoAuthInfo vodAuthInfo);

        bool ChannelExists(string channel);

        string GetChannelIdByName(string channel);

        ObservableCollection<TwitchVideo> Search(SearchParameters searchParams);
    }
}