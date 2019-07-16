using Device.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Usb.Net
{
    public interface IUsbDeviceHandler : IDeviceHandler
    {
        IUsbInterface ReadUsbInterface { get; set; }
        IList<IUsbInterface> UsbInterfaces { get; }
        IUsbInterface WriteUsbInterface { get; set; }
        /// <summary>
        /// TODO: Why is this here?
        /// </summary>
        Task<ConnectedDeviceDefinitionBase> GetConnectedDeviceDefinitionAsync();
    }
}