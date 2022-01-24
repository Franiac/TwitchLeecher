using System.Collections.Generic;
using System.Collections.ObjectModel;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IApiService
    {
        TwitchAuthInfo ValidateAuthentication(string accessToken);

        void RevokeAuthentication(string accessToken);

        TwitchVideoAuthInfo GetVodAuthInfo(string id);

        Dictionary<TwitchVideoQuality, string> GetPlaylistInfo(string vodId, TwitchVideoAuthInfo vodAuthInfo);

        bool ChannelExists(string channel);

        string GetChannelIdByName(string channel);

        ObservableCollection<TwitchVideo> Search(SearchParameters searchParams);
    }
}