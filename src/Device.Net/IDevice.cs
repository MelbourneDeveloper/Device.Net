using System;
using System.Threading;
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
        Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Write a page of data. Warning: this is not thread safe. WriteAndReadAsync() should be preferred.
        /// </summary>
        Task<uint> WriteAsync(byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Close any existing connections and reinitialize the device. 
        /// </summary>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Write a page of data and then wait for the device to return a page. If the implementation derives from DeviceBase, this method is thread safe.
        /// </summary>
        Task<TransferResult> WriteAndReadAsync(byte[] writeBuffer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Device unique OS level Id for the type of device. The device should have been constructed with this Id. It is used to initialize the device.
        /// </summary>
        string DeviceId { get; }

        /// <summary>
        /// Information about the device. This information should be collected from initialization and will be null before initialization and after disposal
        /// </summary>
        ConnectedDeviceDefinition ConnectedDeviceDefinition { get; }

        /// <summary>
        /// Closes the device, but allows for it to be reopened at a later point in time (as opposed to disposing)
        /// </summary>
        void Close();

        /// <summary>
        /// Flushes the device. Note: Only available for serial port devices currently
        /// </summary>
        Task Flush(CancellationToken cancellationToken);
    }
}