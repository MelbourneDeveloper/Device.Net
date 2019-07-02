using System.Collections.Generic;

namespace Usb.Net.Windows
{
    public interface IUsbInterface
    {
        IUsbInterfaceEndpoint ReadEndpoint { get; set; }
        IList<IUsbInterfaceEndpoint> UsbInterfaceEndpoints { get; }
        IUsbInterfaceEndpoint WriteEndpoint { get; set; }
    }
}