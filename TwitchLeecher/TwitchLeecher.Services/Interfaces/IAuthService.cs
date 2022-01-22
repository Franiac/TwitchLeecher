namespace TwitchLeecher.Services.Interfaces
{
    public interface IAuthService
    {
        string GetUsername();

        bool ValidateAuthentication(string accessToken);

        void RevokeAuthentication();
    }
}