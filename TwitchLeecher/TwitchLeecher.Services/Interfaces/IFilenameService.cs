using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IFilenameService
    {
        string SubstituteWildcards(string filename, TwitchVideo video);

        string SubstituteInvalidChars(string filename, string replaceStr);
    }
}