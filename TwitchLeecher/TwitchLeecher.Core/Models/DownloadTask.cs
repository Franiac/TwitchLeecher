using System;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchLeecher.Core.Models
{
    public class DownloadTask
    {
        public DownloadTask(CancellationTokenSource cancellationTokenSource, params Task[] tasks)
        {
            Tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
            CancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));
        }

        public Task[] Tasks { get; private set; }

        public CancellationTokenSource CancellationTokenSource { get; private set; }
    }
}