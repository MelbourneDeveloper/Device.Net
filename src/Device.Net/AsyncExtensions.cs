using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Net
{
    public static class AsyncExtensions
    {

        /// <summary>
        /// TODO: Why do I need to do this? Why doesn't linq have this?
        /// </summary>
        public static async Task<T> FirstOrDefaultAsync<T>(this IEnumerable<T> enumerable, Func<T, Task<bool>> predicate)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (var item in enumerable)
            {
                if (await predicate(item))
                {
                    return item;
                }
            }

            return default;
        }
    }
}
