
#if!NET45
using System;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Device.Net
{
    /// <summary> 
    /// Provides helpers for all platforms. 
    /// </summary> 
    public static class Helpers
    {
#if!NET45
        /// <summary>
        /// Create an awaitable task that will return cancelled if the cancellation token requests cancellation
        /// </summary>
        public static Task<T> SynchronizeWithCancellationToken<T>(this Task<T> task, CancellationToken cancellationToken, int delayMilliseconds = 10)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            while (!task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                Thread.Sleep(delayMilliseconds);

                if (cancellationToken.IsCancellationRequested)
                {
                    return Task.FromCanceled<T>(cancellationToken);
                }
            }

            return task;
        }
#endif
    }
}