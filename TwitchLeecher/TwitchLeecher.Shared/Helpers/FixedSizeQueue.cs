using System.Collections.Concurrent;

namespace TwitchLeecher.Shared.Helpers
{
    public class FixedSizeQueue<T> : ConcurrentQueue<T>
    {
        #region Fields

        private readonly object _lockObject = new object();

        #endregion Fields

        #region Constructors

        public FixedSizeQueue(int size)
        {
            Size = size;
        }

        #endregion Constructors

        #region Properties

        public int Size { get; }

        #endregion Properties

        #region Methods

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);

            lock (_lockObject)
            {
                while (Count > Size)
                {
                    TryDequeue(out _);
                }
            }
        }

        #endregion Methods
    }
}