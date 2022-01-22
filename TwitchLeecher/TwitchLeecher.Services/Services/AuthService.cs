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

        private TwitchAuthInfo _authInfo;

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
        }

        #endregion Constructors

        #region Methods

        public string GetUsername()
        {
            return _authInfo?.Username;
        }

        public bool ValidateAuthentication(string accessToken)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                TwitchAuthInfo authInfo = _apiService.ValidateAuthentication(accessToken);

                if (authInfo != null)
                {
                    _authInfo = authInfo;

                    FireIsAuthorizedChanged();

                    return true;
                }
            }

            RevokeAuthentication(accessToken);

            return false;
        }

        public void RevokeAuthentication()
        {
            RevokeAuthentication(_authInfo?.AccessToken);
        }

        public void RevokeAuthentication(string accessToken)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                _apiService.RevokeAuthentication(accessToken);
            }

            _authInfo = null;

            FireIsAuthorizedChanged();
        }

        private void FireIsAuthorizedChanged()
        {
            _runtimeDataService.RuntimeData.AccessToken = _authInfo?.AccessToken;
            _runtimeDataService.Save();

            _eventAggregator.GetEvent<AuthenticatedChangedEvent>().Publish(_authInfo != null);
        }

        #endregion Methods
    }
}