using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbDeviceHandler : UsbDeviceHandlerBase, IUsbDeviceHandler
    {
        private bool disposed;
        private readonly SafeFileHandle DefaultInterfaceHandle;

        internal WindowsUsbDeviceHandler(SafeFileHandle defaultInterfaceHandle)
        {
            DefaultInterfaceHandle = defaultInterfaceHandle;
        }

        public async Task InitializeAsync()
        {
            await Task.Run(() =>
            {

                //Get the first (default) interface
                //TODO: It seems like there isn't a way to get other interfaces here... 😞
                var defaultInterface = GetInterface(DefaultInterfaceHandle);

                UsbInterfaces.Add(defaultInterface);
                ReadUsbInterface = defaultInterface;
                WriteUsbInterface = defaultInterface;

                byte i = 0;
                while (true)
                {
                    var isSuccess = WinUsbApiCalls.WinUsb_GetAssociatedInterface(DefaultInterfaceHandle, i, out var interfacePointer);
                    if (!isSuccess)
                    {
                        var errorCode = Marshal.GetLastWin32Error();
                        if (errorCode == APICalls.ERROR_NO_MORE_ITEMS) break;

                        throw new Exception($"Could not enumerate interfaces for device. Error code: { errorCode}");
                    }

                    var associatedInterface = GetInterface(interfacePointer);

                    //TODO: this is bad design. The handler should be taking care of this
                    UsbInterfaces.Add(associatedInterface);

                    i++;
                }
            });
        }

        private static WindowsUsbInterface GetInterface(SafeFileHandle interfaceHandle)
        {
            //TODO: Where is the logger/tracer?
            var retVal = new WindowsUsbInterface(null, null) { Handle = interfaceHandle };
            var isSuccess = WinUsbApiCalls.WinUsb_QueryInterfaceSettings(interfaceHandle, 0, out var interfaceDescriptor);
            WindowsDeviceBase.HandleError(isSuccess, "Couldn't query interface");

            for (byte i = 0; i < interfaceDescriptor.bNumEndpoints; i++)
            {
                isSuccess = WinUsbApiCalls.WinUsb_QueryPipe(interfaceHandle, 0, i, out var pipeInfo);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't query endpoint");
                retVal.UsbInterfaceEndpoints.Add(new WindowsUsbInterfaceEndpoint(pipeInfo.PipeId));
            }

            return retVal;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            DefaultInterfaceHandle.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
