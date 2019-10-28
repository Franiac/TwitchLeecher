using System;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IFilenameService
    {
        string SubstituteWildcards(string filename, string folder, FilenameWildcards.IsFileNameUsedsDelegate IsFileNameUsed, TwitchVideo video, TwitchVideoQuality quality = null, TimeSpan? cropStart = null, TimeSpan? cropEnd = null);

        string SubstituteInvalidChars(string filename, string replaceStr);

        string EnsureExtension(string filename, bool disableConversion);
    }
}