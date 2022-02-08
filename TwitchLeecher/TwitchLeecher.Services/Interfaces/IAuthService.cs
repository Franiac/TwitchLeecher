namespace TwitchLeecher.Services.Interfaces
{
    public interface IAuthService
    {
        bool IsAuthenticatedSubOnly { get; }

        bool ValidateAuthentication(string accessToken, bool subOnly);

        void RevokeAuthentication();

        void RevokeAuthenticationSubOnly();
    }
}