using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Usb.Net.Windows
{
    internal class UsbInterface : IDisposable
    {
        #region Fields
        private bool _IsDisposed;
        #endregion

        #region Public Properties
        public SafeFileHandle Handle { get; set; }
        public USB_INTERFACE_DESCRIPTOR USB_INTERFACE_DESCRIPTOR { get; set; }
        public List<UsbInterfacePipe> UsbInterfacePipes { get; } = new List<UsbInterfacePipe>();
        public UsbInterfacePipe ReadPipe => UsbInterfacePipes.FirstOrDefault(p => p.IsRead);
        public UsbInterfacePipe WritePipe => UsbInterfacePipes.FirstOrDefault(p => p.IsWrite);
        #endregion

        #region Public Methods
        public void Dispose()
        {
            if (_IsDisposed) return;
            _IsDisposed = true;

            //This is a native resource, so the IDisposable pattern should probably be implemented...
            var isSuccess = WinUsbApiCalls.WinUsb_Free(Handle);
            WindowsDeviceBase.HandleError(isSuccess, "Interface could not be disposed");
        }
        #endregion
    }
}
