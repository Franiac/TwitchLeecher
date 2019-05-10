﻿using System;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IFilenameService
    {
        string SubstituteWildcards(string filename, TwitchVideo video, TwitchVideoQuality quality = null, TimeSpan? cropStart = null, TimeSpan? cropEnd = null, int? splitPartNumber = null);

        string SubstituteInvalidChars(string filename, string replaceStr);
    }
}