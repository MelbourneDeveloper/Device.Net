using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{
    /// <summary> 
    /// Provides helpers for all platforms. 
    /// </summary> 
    public static class Helpers
    {
        public static bool ContainsIgnoreCase(this string paragraph, string word)
        {
            return ParsingCulture.CompareInfo.IndexOf(paragraph, word, CompareOptions.IgnoreCase) >= 0;
        }

        public static string GetHex(uint? id)
        {
            //TODO: Fix code rules here
            return id?.ToString("X").ToLower().PadLeft(4, '0');
        }

        public static CultureInfo ParsingCulture { get; } = new CultureInfo("en-US");

        public static async Task SynchronizeWithCancellationToken(this Task task, CancellationToken cancellationToken = default)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            var cancelTask = Task.Run(() =>
            {
                while (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    //TODO: Soft code this
                    Task.Delay(10);

                    if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(Messages.ErrorMessageOperationCanceled);
                }
            });

            await Task.WhenAny(new Task[] { task, cancelTask });
        }
    }
}