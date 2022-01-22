using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    internal interface IApiService
    {
        TwitchAuthInfo ValidateAuthentication(string accessToken);

        void RevokeAuthentication(string accessToken);
    }
}