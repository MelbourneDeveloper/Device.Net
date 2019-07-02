using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbInterface : IDisposable, IUsbInterface
    {
        #region Fields
        private IUsbInterfaceEndpoint _ReadEndpoint;
        private IUsbInterfaceEndpoint _WriteEndpoint;
        private bool _IsDisposed;
        private readonly List<WindowsUsbInterfaceEndpoint> _UsbInterfaceEndpoints = new List<WindowsUsbInterfaceEndpoint>();
        #endregion

        public ILogger Logger { get; }
        public ITracer Tracer { get; }

        #region Internal Properties
        /// <summary>
        /// TODO: Make private?
        /// </summary>
        internal SafeFileHandle Handle { get; set; }
        #endregion

        public WindowsUsbInterface(ILogger logger, ITracer tracer)
        {
            Tracer = tracer;
            Logger = logger;
        }

        #region Public Properties
        public IList<IUsbInterfaceEndpoint> UsbInterfaceEndpoints => (IList<IUsbInterfaceEndpoint>)_UsbInterfaceEndpoints;

        public IUsbInterfaceEndpoint ReadEndpoint
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

        public IUsbInterfaceEndpoint WriteEndpoint
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
        public async Task<byte[]> ReadAsync(uint bufferLength)
        {
            return await Task.Run(() =>
            {
                var bytes = new byte[bufferLength];
                var isSuccess = WinUsbApiCalls.WinUsb_ReadPipe(Handle, ReadEndpoint.PipeId, bytes, bufferLength, out var bytesRead, IntPtr.Zero);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't read data");
                Tracer?.Trace(false, bytes);
                return bytes;
            });
        }

        public async Task WriteAsync(byte[] data)
        {
            await Task.Run(() =>
            {
                var isSuccess = WinUsbApiCalls.WinUsb_WritePipe(Handle, WriteEndpoint.PipeId, data, (uint)data.Length, out var bytesWritten, IntPtr.Zero);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't write data");
                Tracer?.Trace(true, data);
            });
        }

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
