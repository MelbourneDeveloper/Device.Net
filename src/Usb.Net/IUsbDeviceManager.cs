using Device.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Usb.Net
{
    public interface IUsbInterfaceManager : IDeviceHandler
    {
        /// <summary>
        /// Usb interface for reading from the device. Note: this will default to the first read Bulk interface. If this is incorrect, inspect the UsbInterfaces property.
        /// </summary>
        IUsbInterface ReadUsbInterface { get; set; }
        /// <summary>
        /// Usb interface for writing to the device. Note: this will default to the first write Bulk interface. If this is incorrect, inspect the UsbInterfaces property.
        /// </summary>
        IUsbInterface WriteUsbInterface { get; set; }

        //TODO: This should be a read only collection
        IList<IUsbInterface> UsbInterfaces { get; }

        /// <summary>
        /// TODO: This shouldn't be here. Don't use this
        /// </summary>
        Task<ConnectedDeviceDefinition> GetConnectedDeviceDefinitionAsync();
    }
}