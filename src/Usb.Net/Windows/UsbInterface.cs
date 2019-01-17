using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Usb.Net.Windows
{
    public class UsbInterface : IDisposable
    {
        #region Fields
        private UsbInterfacePipe _ReadPipe;
        private UsbInterfacePipe _WritePipe;
        #endregion

        #region Internal Properties
        internal SafeFileHandle Handle { get; set; }
        #endregion

        #region Public Properties
        public List<UsbInterfacePipe> UsbInterfacePipes { get; } = new List<UsbInterfacePipe>();

        public UsbInterfacePipe ReadPipe
        {
            get
            {
                //This is a bit stinky but should work
                if (_ReadPipe == null)
                {
                    _ReadPipe = UsbInterfacePipes.FirstOrDefault(p => p.IsRead);
                }

                return _ReadPipe;
            }
            set
            {
                if (!UsbInterfacePipes.Contains(value)) throw new Exception("This pipe is not contained in the list of valid pipes");
                _ReadPipe = value;
            }
        }

        public UsbInterfacePipe WritePipe
        {
            get
            {
                //This is a bit stinky but should work
                if (_WritePipe == null)
                {
                    _WritePipe = UsbInterfacePipes.FirstOrDefault(p => p.IsWrite);
                }

                return _WritePipe;
            }
            set
            {
                if (!UsbInterfacePipes.Contains(value)) throw new Exception("This pipe is not contained in the list of valid pipes");
                _WritePipe = value;
            }
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            var isSuccess = WinUsbApiCalls.WinUsb_Free(Handle);
            WindowsDeviceBase.HandleError(isSuccess, "Interface could not be disposed");
        }
        #endregion
    }
}
