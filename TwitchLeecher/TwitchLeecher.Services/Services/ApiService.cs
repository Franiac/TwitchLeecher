using Newtonsoft.Json.Linq;
using System;
using System.Net;
using TwitchLeecher.Core;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;

namespace TwitchLeecher.Services.Services
{
    internal class ApiService : IApiService
    {
        #region Methods

        public TwitchAuthInfo ValidateAuthentication(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return null;
            }

            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add(HttpRequestHeader.Authorization, $"Bearer { accessToken }");

                string jsonStr = null;

                try
                {
                    jsonStr = wc.DownloadString("https://id.twitch.tv/oauth2/validate");
                }
                catch (WebException)
                {
                    // Any WebException indicates that the access token could not be verified
                    return null;
                }

                if (!string.IsNullOrWhiteSpace(jsonStr))
                {
                    JObject json = JObject.Parse(jsonStr);

                    if (json != null)
                    {
                        string login = json.Value<string>("login");
                        string userId = json.Value<string>("user_id");
                        string clientId = json.Value<string>("client_id");

                        if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(clientId) && clientId.Equals(Constants.ClientId, StringComparison.OrdinalIgnoreCase))
                        {
                            return new TwitchAuthInfo(accessToken, login, userId);
                        }
                    }
                }
            }

            return null;
        }

        public void RevokeAuthentication(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return;
            }

            using (WebClient wc = new WebClient())
            {
                try
                {
                    _ = wc.UploadString($"https://id.twitch.tv/oauth2/revoke?client_id={ Constants.ClientId }&token={ accessToken }", string.Empty);
                }
                catch (WebException)
                {
                    // Ignore potentionally failed revoke requests
                }
                
            }
        }

        #endregion Methods
    }
}