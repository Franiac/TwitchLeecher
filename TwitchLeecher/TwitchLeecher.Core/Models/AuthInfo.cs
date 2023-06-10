using System.Xml.Linq;
using TwitchLeecher.Shared.Extensions;

namespace TwitchLeecher.Core.Models
{
    public class AuthInfo
    {
        #region Constants

        public const string AUTHENTICATION_EL = "Authentication";

        private const string AUTHENTICATION_ACCESSTOKEN_EL = "AccessToken";
        private const string AUTHENTICATION_ACCESSTOKENSUBONLY_EL = "AccessTokenSubOnly";

        #endregion Constants

        #region Properties

        public string AccessToken { get; set; }

        public string AccessTokenSubOnly { get; set; }

        #endregion Properties

        #region Methods

        public XElement GetXml()
        {
            if (string.IsNullOrWhiteSpace(AccessToken) && string.IsNullOrWhiteSpace(AccessTokenSubOnly))
            {
                return null;
            }

            XElement authenticationEl = new XElement(AUTHENTICATION_EL);

            if (!string.IsNullOrWhiteSpace(AccessToken))
            {
                XElement accessTokenEl = new XElement(AUTHENTICATION_ACCESSTOKEN_EL);
                accessTokenEl.SetValue(AccessToken);
                authenticationEl.Add(accessTokenEl);
            }

            return authenticationEl;
        }

        #endregion Methods

        #region Static Methods

        public static AuthInfo GetFromXml(XElement authenticationEl)
        {
            AuthInfo runtimeAuthInfo = new AuthInfo();

            bool entryFound = false;

            if (authenticationEl != null)
            {
                XElement accessTokenEl = authenticationEl.Element(AUTHENTICATION_ACCESSTOKEN_EL);

                if (accessTokenEl != null)
                {
                    try
                    {
                        runtimeAuthInfo.AccessToken = accessTokenEl.GetValueAsString();
                        entryFound = true;
                    }
                    catch
                    {
                        // Malformed XML
                        return null;
                    }
                }
            }

            if (entryFound)
            {
                return runtimeAuthInfo;
            }
            else
            {
                return null;
            }
        }

        #endregion Static Methods
    }
}