using System;

namespace TwitchLeecher.Shared.Events
{
    public interface IDelegateReference
    {
        #region Properties

        Delegate Target { get; }

        #endregion Properties
    }
}