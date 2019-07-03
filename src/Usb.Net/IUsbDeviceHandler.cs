using System.Collections.Generic;

namespace Usb.Net
{
    public interface IUsbDeviceHandler 
    {
        IUsbInterface ReadUsbInterface { get; set; }
        IList<IUsbInterface> UsbInterfaces { get; }
        IUsbInterface WriteUsbInterface { get; set; }
    }
}