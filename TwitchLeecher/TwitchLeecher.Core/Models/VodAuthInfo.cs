using System;

namespace TwitchLeecher.Core.Models
{
    public class VodAuthInfo
    {
        #region Constructors

        public VodAuthInfo(string token, string signature, bool privileged, bool subOnly)
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
            this.Privileged = privileged;
            this.SubOnly = subOnly;
        }

        #endregion Constructors

        #region Properties

        public string Token { get; private set; }

        public string Signature { get; private set; }

        public bool Privileged { get; set; }

        public bool SubOnly { get; set; }

        #endregion Properties
    }
}