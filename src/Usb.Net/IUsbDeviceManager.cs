using Device.Net;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net
{
    /// <summary>
    /// Manages USB interfaces
    /// </summary>
    public interface IUsbInterfaceManager : IDisposable
    {
        /// <summary>
        /// Usb interface for reading from the device. Note: this will default to the first read Bulk interface. If this is incorrect, inspect the UsbInterfaces property.
        /// </summary>
        IUsbInterface? ReadUsbInterface { get; set; }
        /// <summary>
        /// Usb interface for writing to the device. Note: this will default to the first write Bulk interface. If this is incorrect, inspect the UsbInterfaces property.
        /// </summary>
        IUsbInterface? WriteUsbInterface { get; set; }

        //TODO: This should be a read only collection
        IList<IUsbInterface> UsbInterfaces { get; }

        /// <summary>
        /// TODO: This shouldn't be here. Don't use this
        /// </summary>
        Task<ConnectedDeviceDefinition> GetConnectedDeviceDefinitionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Maximum write buffer size
        /// </summary>
        ushort WriteBufferSize { get; }

        /// <summary>
        /// Maximum read buffer size
        /// </summary>
        ushort ReadBufferSize { get; }

        /// <summary>
        /// Whether or not the manager is initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Initialize the manager
        /// </summary>
        /// <param name="cancellationToken">Allows you to cancel the operation</param>
        /// <returns></returns>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Close the manager
        /// </summary>
        void Close();
    }
}