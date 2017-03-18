using System.Collections.Concurrent;

namespace TwitchLeecher.Shared.Helpers
{
    public class FixedSizeQueue<T>
    {
        #region Fields

        private readonly object _queueLockObject;

        private ConcurrentQueue<T> _queue;

        private int _size;

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

            _size = size;
        }

        #endregion Constructors

        #region Properties

        public int Size
        {
            get
            {
                return _size;
            }
        }

        #endregion Properties

        #region Methods

        public void Enqueue(T obj)
        {
            lock (_queueLockObject)
            {
                _queue.Enqueue(obj);

                while (_queue.Count > _size)
                {
                    _queue.TryDequeue(out T outObj);
                }
            }
        }

        #endregion Methods
    }
}