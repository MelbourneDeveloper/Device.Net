using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Usb.Net.Windows
{
    internal class UsbInterface : IDisposable
    {
        public SafeFileHandle Handle { get; set; }
        public WinUsbApiCalls.USB_INTERFACE_DESCRIPTOR USB_INTERFACE_DESCRIPTOR { get; set; }
        public List<UsbInterfacePipe> UsbInterfacePipes { get; } = new List<UsbInterfacePipe>();
        public UsbInterfacePipe ReadPipe => UsbInterfacePipes.FirstOrDefault(p => p.IsRead);
        public UsbInterfacePipe WritePipe => UsbInterfacePipes.FirstOrDefault(p => p.IsWrite);

        public void Dispose()
        {
            var isSuccess = WinUsbApiCalls.WinUsb_Free(Handle);
            if(!isSuccess)
            {
                var errorCode = Marshal.GetLastWin32Error();
                throw new Exception($"Interface could not be disposed. Error code {errorCode}.");
            }
        }
    }
}
