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
        private UsbInterfaceEndpoint _ReadEndpoint;
        private UsbInterfaceEndpoint _WriteEndpoint;
        private bool _IsDisposed;
        #endregion

        #region Internal Properties
        internal SafeFileHandle Handle { get; set; }
        #endregion

        #region Public Properties
        public List<UsbInterfaceEndpoint> UsbInterfaceEndpoints { get; } = new List<UsbInterfaceEndpoint>();

        public UsbInterfaceEndpoint ReadEndpoint
        {
            get
            {
                //This is a bit stinky but should work
                if (_ReadEndpoint == null)
                {
                    _ReadEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsRead);
                }

                return _ReadEndpoint;
            }
            set
            {
                if (!UsbInterfaceEndpoints.Contains(value)) throw new Exception("This endpoint is not contained in the list of valid endpoints");
                _ReadEndpoint = value;
            }
        }

        public UsbInterfaceEndpoint WriteEndpoint
        {
            get
            {
                //This is a bit stinky but should work
                if (_WriteEndpoint == null)
                {
                    _WriteEndpoint = UsbInterfaceEndpoints.FirstOrDefault(p => p.IsWrite);
                }

                return _WriteEndpoint;
            }
            set
            {
                if (!UsbInterfaceEndpoints.Contains(value)) throw new Exception("This endpoint is not contained in the list of valid endpoints");
                _WriteEndpoint = value;
            }
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            if (_IsDisposed) return;
            _IsDisposed = true;

            //This is a native resource, so the IDisposable pattern should probably be implemented...
            var isSuccess = WinUsbApiCalls.WinUsb_Free(Handle);
            WindowsDeviceBase.HandleError(isSuccess, "Interface could not be disposed");

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
