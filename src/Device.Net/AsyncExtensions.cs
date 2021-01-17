using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{
    internal static class AsyncExtensions
    {

        /// <summary>
        /// TODO: Why do I need to do this? Why doesn't linq have this?
        /// </summary>
        public static async Task<T> FirstOrDefaultAsync<T>(this IEnumerable<T> enumerable, Func<T, Task<bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));

            if (predicate == null) return enumerable.FirstOrDefault();

            foreach (var item in enumerable)
            {
                if (cancellationToken.IsCancellationRequested) return default;

                if (await predicate(item).ConfigureAwait(false))
                {
                    return item;
                }
            }

            return default;
        }
    }
}
