using System;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDevice : IDisposable
    {
        /// <summary>
        /// Occurs after the device has successfully connected. Note: this can be called multiple times and will occurr every time  InitializeAsync is called successfull. 
        /// </summary>
        event EventHandler Connected;

        /// <summary>
        /// Placeholder. This is not currently being used. Please do not rely on this at the moment.
        /// </summary>
        event EventHandler Disconnected;

        /// <summary>
        /// Checks to see if the device has been successfully connected. Note: check the implementation to see if this method is actually asking the device whether it is still connected or not
        /// </summary>
        Task<bool> GetIsConnectedAsync();

        /// <summary>
        /// Read a page of data
        /// </summary>
        Task<byte[]> ReadAsync();

        /// <summary>
        /// Write a page of data
        /// </summary>
        Task WriteAsync(byte[] data);

        /// <summary>
        /// Dispose of any existing connections and reinitialize the device
        /// </summary>
        Task InitializeAsync();
    }
}