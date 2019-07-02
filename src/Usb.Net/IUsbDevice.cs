using Device.Net;
using System.Collections.Generic;

namespace Usb.Net
{
    public interface IUsbDevice : IDevice
    {
        IUsbInterface ReadUsbInterface { get; set; }
        IList<IUsbInterface> UsbInterfaces { get; }
        IUsbInterface WriteUsbInterface { get; set; }
    }
}