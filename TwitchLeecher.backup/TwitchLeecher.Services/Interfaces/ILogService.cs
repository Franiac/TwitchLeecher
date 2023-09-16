using System;

namespace TwitchLeecher.Services.Interfaces
{
    public interface ILogService
    {
        string LogException(Exception ex);
    }
}