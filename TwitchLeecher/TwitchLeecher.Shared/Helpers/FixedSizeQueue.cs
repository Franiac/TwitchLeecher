using System.Collections.Concurrent;

namespace TwitchLeecher.Shared.Helpers
{
    public class FixedSizeQueue<T>
    {
        #region Fields

        private readonly object _queueLockObject;

        private ConcurrentQueue<T> _queue;

        #endregion Fields

        #region Constructors

        public FixedSizeQueue(int size)
        {
            _queueLockObject = new object();
            _queue = new ConcurrentQueue<T>();

            if (size < 1)
            {
                size = 1;
            }

            Size = size;
        }

        #endregion Constructors

        #region Properties

        public int Size { get; }

        #endregion Properties

        #region Methods

        public void Enqueue(T obj)
        {
            lock (_queueLockObject)
            {
                _queue.Enqueue(obj);

                while (_queue.Count > Size)
                {
                    _queue.TryDequeue(out T outObj);
                }
            }
        }

        #endregion Methods
    }
}