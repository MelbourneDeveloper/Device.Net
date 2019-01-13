using System;
using System.Threading.Tasks;

namespace Device.Net
{
    public interface IDevice : IDisposable
    {
        /// <summary>
        /// Whether or not the device has been successfully initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Read a page of data. Warning: this is not thread safe. WriteAndReadAsync() should be preferred.
        /// </summary>
        Task<byte[]> ReadAsync();

        /// <summary>
        /// Write a page of data. Warning: this is not thread safe. WriteAndReadAsync() should be preferred.
        /// </summary>
        Task WriteAsync(byte[] data);

        /// <summary>
        /// Dispose of any existing connections and reinitialize the device. 
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Write a page of data and then wait for the device to return a page. If the implementation derives from DeviceBase, this method is thread safe.
        /// </summary>
        Task<byte[]> WriteAndReadAsync(byte[] writeBuffer);

        /// <summary>
        /// Device unique OS level Id for the type of device. The device should have been constructed with this Id. It is used to initialize the device.
        /// </summary>
        string DeviceId { get; }

        /// <summary>
        /// Information about the device. This information should be collected from initialization
        /// </summary>
        DeviceDefinitionBase DeviceDefinition { get;  }
    }
}