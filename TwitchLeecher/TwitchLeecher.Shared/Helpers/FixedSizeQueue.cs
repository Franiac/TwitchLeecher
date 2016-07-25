using System.Collections.Concurrent;

namespace TwitchLeecher.Shared.Helpers
{
    public class FixedSizeQueue<T>
    {
        #region Fields

        private readonly object queueLockObject;

        private ConcurrentQueue<T> queue;

        private int size;

        #endregion Fields

        #region Constructors

        public FixedSizeQueue(int size)
        {
            this.queueLockObject = new object();
            this.queue = new ConcurrentQueue<T>();

            if (size < 1)
            {
                size = 1;
            }

            this.size = size;
        }

        #endregion Constructors

        #region Properties

        public int Size
        {
            get
            {
                return this.size;
            }
        }

        #endregion Properties

        #region Methods

        public void Enqueue(T obj)
        {
            lock (this.queueLockObject)
            {
                this.queue.Enqueue(obj);

                while (this.queue.Count > this.size)
                {
                    T outObj;
                    this.queue.TryDequeue(out outObj);
                }
            }
        }

        #endregion Methods
    }
}