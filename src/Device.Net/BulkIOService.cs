using Device.Net.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net
{
    /// <summary>
    /// Wraps a device and allows it to be used like a stream
    /// </summary>
    public class BulkIOService
    {
        #region Public Properties
        public IDevice Device { get;  }
        public int ReadBufferSize { get; }
        #endregion

        #region Constructor
        public BulkIOService(IDevice device, int readBufferSize)
        {
            Device = device;
            ReadBufferSize = readBufferSize;
        }
        #endregion

        #region Public Methods
        public async Task<int> ReadAsync(byte[] buff, int offset, int len)
        {
            if (buff == null) throw new ArgumentNullException(nameof(buff));

            if (buff.Length - offset < len)
            {
                throw new ValidationException("Index out of bounds");
            }

            var totalRead = 0;

            while (totalRead < len)
            {
                // Getting some data.
                var data = await Device.ReadAsync();

                if (data.Length > len - totalRead)
                {
                    // Out of bounds.
                    // Cut the overflowing result.
                    data = data.Take(len - totalRead).ToArray();
                }

                // Copying buffer.
                for (var i = 0; i < data.Length; ++i)
                {
                    buff[totalRead + offset] = data[i];
                    ++totalRead;
                }

                if (data.Length < ReadBufferSize)
                {
                    // Last buffer.
                    break;
                }
            }

            return totalRead;
        }

        public async Task WriteAsync(byte[] data, int offset, int len)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            // Some checking.
            if (data.Length - offset < len)
            {
                throw new ValidationException("Index out of bounds");
            }

            // Determining buffer size.
            byte[] writeBuff;

            if (offset == 0 && len == data.Length)
            {
                writeBuff = data;
            }
            else
            {
                writeBuff = new byte[len];

                // Copying buffer.
                for (var i = 0; i < len; ++i)
                {
                    writeBuff[i] = data[offset + i];
                }
            }

            await Device.WriteAsync(writeBuff);
        }
        #endregion
    }
}
