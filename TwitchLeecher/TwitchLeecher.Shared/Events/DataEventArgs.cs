using System;

namespace TwitchLeecher.Shared.Events
{
    public class DataEventArgs<TData> : EventArgs
    {
        #region Fields

        private readonly TData value;

        #endregion Fields

        #region Constructors

        public DataEventArgs(TData value)
        {
            this.value = value;
        }

        #endregion Constructors

        #region Properties

        public TData Value
        {
            get { return value; }
        }

        #endregion Properties
    }
}