using System;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchLeecher.Services.Models
{
    internal class DownloadTask
    {
        public DownloadTask(Task task, Task continueTask, CancellationTokenSource cancellationTokenSource)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (continueTask == null)
            {
                throw new ArgumentNullException(nameof(continueTask));
            }
            if (cancellationTokenSource == null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }

            this.Task = task;
            this.ContinueTask = continueTask;
            this.CancellationTokenSource = cancellationTokenSource;
        }

        public Task Task { get; private set; }

        public Task ContinueTask { get; private set; }

        public CancellationTokenSource CancellationTokenSource { get; private set; }
    }
}