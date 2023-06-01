using System.Collections.Generic;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IPreferencesService
    {
        Preferences CurrentPreferences { get; }
        IEnumerable<string> AvailableThemes { get; }

        bool IsChannelInFavourites(string channel);

        void Save(Preferences preferences);

        Preferences CreateDefault();
    }
}