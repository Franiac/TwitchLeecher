using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IPreferencesService
    {
        Preferences CurrentPreferences { get; }

        bool IsChannelInFavourites(string channel);

        void Save(Preferences preferences);

        Preferences CreateDefault();
    }
}