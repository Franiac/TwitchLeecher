using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IUpdateService
    {
        UpdateInfo CheckForUpdate();
    }
}