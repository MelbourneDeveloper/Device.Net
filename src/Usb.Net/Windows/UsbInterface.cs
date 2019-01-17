using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Usb.Net.Windows
{
    public class UsbInterface : IDisposable
    {
        internal SafeFileHandle Handle { get; set; }
        //public WinUsbApiCalls.USB_INTERFACE_DESCRIPTOR USB_INTERFACE_DESCRIPTOR { get; set; }
        public List<UsbInterfacePipe> UsbInterfacePipes { get; } = new List<UsbInterfacePipe>();
        public UsbInterfacePipe ReadPipe => UsbInterfacePipes.FirstOrDefault(p => p.IsRead);
        public UsbInterfacePipe WritePipe => UsbInterfacePipes.FirstOrDefault(p => p.IsWrite);

        public void Dispose()
        {
            var isSuccess = WinUsbApiCalls.WinUsb_Free(Handle);
            WindowsDeviceBase.HandleError(isSuccess, "Interface could not be disposed");
        }
    }
}
