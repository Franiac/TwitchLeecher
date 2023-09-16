using TwitchLeecher.Core.Events;
using TwitchLeecher.Core.Models;
using TwitchLeecher.Services.Interfaces;
using TwitchLeecher.Shared.Events;

namespace TwitchLeecher.Services.Services
{
    internal class AuthService : IAuthService
    {
        #region Fields

        private readonly IApiService _apiService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRuntimeDataService _runtimeDataService;

        private bool _isAuthenticatedSubOnly;

        #endregion Fields

        #region Constructors

        public AuthService(
            IApiService apiService,
            IEventAggregator eventAggregator,
            IRuntimeDataService runtimeDataService)
        {
            _apiService = apiService;
            _eventAggregator = eventAggregator;
            _runtimeDataService = runtimeDataService;

            _eventAggregator.GetEvent<SubOnlyAuthChangedEvent>().Subscribe(SubOnlyAuthChanged);
        }

        #endregion Constructors

        #region Properties

        public bool IsAuthenticatedSubOnly
        {
            get
            {
                return _isAuthenticatedSubOnly;
            }
        }

        #endregion Properties

        #region Methods

        private void SubOnlyAuthChanged(bool isAuthenticatedSubOnly)
        {
            _isAuthenticatedSubOnly = isAuthenticatedSubOnly;
        }

        private AuthInfo GetCurrentAuthInfo()
        {
            AuthInfo curAuthInfo = _runtimeDataService.RuntimeData.AuthInfo;

            if (curAuthInfo == null)
            {
                return new AuthInfo();
            }

            return curAuthInfo;
        }

        public bool ValidateAuthentication(string accessToken, bool subOnly)
        {
            AuthInfo authInfo = GetCurrentAuthInfo();

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                if (_apiService.ValidateAuthentication(accessToken, subOnly))
                {
                    if (subOnly)
                    {
                        authInfo.AccessTokenSubOnly = accessToken;
                    }
                    else
                    {
                        authInfo.AccessToken = accessToken;
                    }

                    FireIsAuthorizedChanged(authInfo);

                    return true;
                }
            }

            _apiService.RevokeAuthentication(accessToken, subOnly);

            if (subOnly)
            {
                RevokeAuthenticationSubOnly();
            }
            else
            {
                RevokeAuthentication();
            }

            return false;
        }

        public void RevokeAuthentication()
        {
            AuthInfo authInfo = GetCurrentAuthInfo();

            if (!string.IsNullOrWhiteSpace(authInfo.AccessTokenSubOnly))
            {
                RevokeAuthentication(authInfo, authInfo.AccessTokenSubOnly, true);
            }

            if (!string.IsNullOrWhiteSpace(authInfo.AccessToken))
            {
                RevokeAuthentication(authInfo, authInfo.AccessToken, false);
            }
        }

        public void RevokeAuthenticationSubOnly()
        {
            AuthInfo authInfo = GetCurrentAuthInfo();

            if (!string.IsNullOrWhiteSpace(authInfo.AccessTokenSubOnly))
            {
                RevokeAuthentication(authInfo, authInfo.AccessTokenSubOnly, true);
            }
        }

        private void RevokeAuthentication(AuthInfo authInfo, string accessToken, bool subOnly)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                _apiService.RevokeAuthentication(accessToken, subOnly);
            }

            if (subOnly)
            {
                authInfo.AccessTokenSubOnly = null;
            }
            else
            {
                authInfo.AccessToken = null;
            }

            FireIsAuthorizedChanged(authInfo);
        }

        private void FireIsAuthorizedChanged(AuthInfo authInfo)
        {
            if (authInfo == null || (string.IsNullOrWhiteSpace(authInfo.AccessToken) && string.IsNullOrWhiteSpace(authInfo.AccessTokenSubOnly)))
            {
                _runtimeDataService.RuntimeData.AuthInfo = null;
            }
            else
            {
                AuthInfo newAuthInfo = new AuthInfo();

                if (!string.IsNullOrWhiteSpace(authInfo.AccessToken))
                {
                    newAuthInfo.AccessToken = authInfo.AccessToken;
                }

                if (!string.IsNullOrWhiteSpace(authInfo.AccessTokenSubOnly))
                {
                    newAuthInfo.AccessTokenSubOnly = authInfo.AccessTokenSubOnly;
                }

                _runtimeDataService.RuntimeData.AuthInfo = newAuthInfo;
            }

            _runtimeDataService.Save();

            _eventAggregator.GetEvent<SubOnlyAuthChangedEvent>().Publish(!string.IsNullOrWhiteSpace(authInfo.AccessTokenSubOnly));
            _eventAggregator.GetEvent<AuthChangedEvent>().Publish(!string.IsNullOrWhiteSpace(authInfo.AccessToken));
        }

        #endregion Methods
    }
}