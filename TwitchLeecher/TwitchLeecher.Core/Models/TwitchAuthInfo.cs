using System;

namespace TwitchLeecher.Core.Models
{
    public class TwitchAuthInfo
    {
        #region Constructors

        public TwitchAuthInfo(string accessToken, string username, string userId)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            AccessToken = accessToken;
            Username = username;
            UserId = userId;
        }

        #endregion Constructors

        #region Properties

        public string AccessToken { get; private set; }

        public string Username { get; private set; }

        public string UserId { get; private set; }

        #endregion Properties
    }
}