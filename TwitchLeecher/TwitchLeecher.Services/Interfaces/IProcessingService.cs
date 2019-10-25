using System;
using TwitchLeecher.Core.Models;

namespace TwitchLeecher.Services.Interfaces
{
    public interface IProcessingService
    {
        string FFMPEGExe { get; }

        void ConcatParts(Action<string> log, Action<string> setStatus, Action<double> setProgress, VodPlaylist vodPlaylist, string concatFile, bool removeFiles = true);

        void ConvertVideo(Action<string> log, Action<string> setStatus, Action<double> setProgress, Action<bool> setIsIndeterminate, string sourceFile, string outputFile, CropInfo cropInfo);
    }
}