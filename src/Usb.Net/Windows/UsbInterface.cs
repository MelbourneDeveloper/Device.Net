using System;
using System.Collections.Generic;
using System.Linq;

namespace Usb.Net.Windows
{
    internal class UsbInterface
    {
        public IntPtr Handle { get; set; }
        public WinUsbApiCalls.USB_INTERFACE_DESCRIPTOR USB_INTERFACE_DESCRIPTOR { get; set; }
        public List<UsbInterfacePipe> UsbInterfacePipes { get; } = new List<UsbInterfacePipe>();
        public UsbInterfacePipe ReadPipe => UsbInterfacePipes.FirstOrDefault(p => p.IsRead);
        public UsbInterfacePipe WritePipe => UsbInterfacePipes.FirstOrDefault(p => p.IsWrite);
    }
}
