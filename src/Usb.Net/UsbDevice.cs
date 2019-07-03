using Device.Net;
using Microsoft.Win32.SafeHandles;
using System;
using System.Threading.Tasks;

namespace Usb.Net
{
    public class UsbDevice : DeviceBase, IUsbDevice
    {
        #region Fields
        private SafeFileHandle _DeviceHandle;
        private bool disposed;
        private bool _IsClosing;
        #endregion

        #region Public Overrride Properties
        public override bool IsInitialized => _DeviceHandle != null && !_DeviceHandle.IsInvalid;
        public IUsbDeviceHandler UsbDeviceHandler { get; }
        public override ushort WriteBufferSize => UsbDeviceHandler.WriteBufferSize;
        public override ushort ReadBufferSize => UsbDeviceHandler.ReadBufferSize;
        #endregion

        #region Constructor
        public UsbDevice(IUsbDeviceHandler usbDeviceHandler) : base()
        {
            UsbDeviceHandler = usbDeviceHandler;
        }
        #endregion

        #region Private Methods

        #endregion

        #region Public Methods
        public async Task InitializeAsync()
        {
            await UsbDeviceHandler.InitializeAsync();
        }

        public override Task<byte[]> ReadAsync()
        {
            return UsbDeviceHandler.ReadUsbInterface.ReadAsync(ReadBufferSize);
        }

        public override Task WriteAsync(byte[] data)
        {
            return UsbDeviceHandler.WriteUsbInterface.WriteAsync(data);
        }

        public void Close()
        {
            if (_IsClosing) return;
            _IsClosing = true;

            try
            {
                if (UsbDeviceHandler != null)
                {
                    foreach (var usbInterface in UsbDeviceHandler.UsbInterfaces)
                    {
                        usbInterface.Dispose();
                    }

                    UsbDeviceHandler.UsbInterfaces.Clear();
                }

                _DeviceHandle?.Dispose();
                _DeviceHandle = null;
            }
            catch (Exception)
            {
                //TODO: Logging
            }

            _IsClosing = false;
        }

        public sealed override void Dispose()
        {
            if (disposed) return;
            disposed = true;

            Close();

            base.Dispose();

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Finalizer
        ~UsbDevice()
        {
            Dispose();
        }
        #endregion
    }
}
