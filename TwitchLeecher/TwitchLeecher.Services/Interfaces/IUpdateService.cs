using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IUpdateService
    {
        bool CheckForUpdate(out UpdateInfo updateInfo);
    }
}