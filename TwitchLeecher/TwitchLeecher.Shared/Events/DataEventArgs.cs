using System;

namespace TwitchLeecher.Shared.Events
{
    public class DataEventArgs<TData> : EventArgs
    {
        #region Fields

        private readonly TData _value;

        #endregion Fields

        #region Constructors

        public DataEventArgs(TData value)
        {
            _value = value;
        }

        #endregion Constructors

        #region Properties

        public TData Value
        {
            get { return _value; }
        }

        #endregion Properties
    }
}