using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IPreferencesService
    {
        Preferences CurrentPreferences { get; }

        void Save(Preferences preferences);

        Preferences CreateDefault();
    }
}