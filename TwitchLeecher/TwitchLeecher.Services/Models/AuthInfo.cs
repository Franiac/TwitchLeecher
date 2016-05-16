using System;

namespace TwitchLeecher.Services.Models
{
    public class AuthInfo
    {
        public AuthInfo(string token, string signature)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrWhiteSpace(signature))
            {
                throw new ArgumentNullException(nameof(signature));
            }

            this.Token = token;
            this.Signature = signature;
        }

        public string Token { get; private set; }

        public string Signature { get; private set; }
    }
}