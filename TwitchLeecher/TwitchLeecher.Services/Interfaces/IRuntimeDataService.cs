using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IRuntimeDataService
    {
        RuntimeData RuntimeData { get; }

        void Save();
    }
}