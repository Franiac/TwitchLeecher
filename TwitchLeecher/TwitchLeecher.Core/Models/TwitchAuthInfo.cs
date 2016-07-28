using System;

namespace TwitchLeecher.Core.Models
{
    public class TwitchAuthInfo
    {
        #region Constructors

        public TwitchAuthInfo(string accessToken, string username)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            this.AccessToken = accessToken;
            this.Username = username;
        }

        #endregion Constructors

        #region Properties

        public string AccessToken { get; private set; }

        public string Username { get; private set; }

        #endregion Properties
    }
}