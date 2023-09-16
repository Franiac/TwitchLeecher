﻿using System;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IProcessingService
    {
        string FFMPEGExe { get; }

        void ConcatParts(Action<string> log, Action<string> setStatus, Action<double> setProgress, TwitchPlaylist vodPlaylist, string concatFile);

        void ConvertVideo(Action<string> log, Action<string> setStatus, Action<double> setProgress, Action<bool> setIsIndeterminate, string sourceFile, string outputFile, CropInfo cropInfo);
    }
}