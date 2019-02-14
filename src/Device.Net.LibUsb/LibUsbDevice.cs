using LibUsbDotNet;
using LibUsbDotNet.LudnMonoLibUsb;
using LibUsbDotNet.Main;
using LibUsbDotNet.WinUsb;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.LibUsb
{
    public class LibUsbDevice : IDevice
    {
        #region Fields
        private UsbEndpointReader _UsbEndpointReader;
        private UsbEndpointWriter _UsbEndpointWriter;
        private int ReadPacketSize;
        private SemaphoreSlim _WriteAndReadLock = new SemaphoreSlim(1, 1);
        private bool disposed;
        #endregion

        #region Public Properties
        public UsbDevice UsbDevice { get; }
        public int VendorId => GetVendorId(UsbDevice);
        public int ProductId => GetProductId(UsbDevice);
        public int Timeout { get; }
        public bool IsInitialized { get; private set; }
        public ConnectedDeviceDefinitionBase ConnectedDeviceDefinition => throw new NotImplementedException();
        public string DeviceId => UsbDevice.DevicePath;
        #endregion

        #region Events
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        #endregion

        #region Constructor
        public LibUsbDevice(UsbDevice usbDevice, int timeout)
        {
            UsbDevice = usbDevice;
            Timeout = timeout;
        }
        #endregion

        #region Implementation
        public void Close()
        {
            UsbDevice?.Close();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            _WriteAndReadLock.Dispose();

            Close();
        }

        public async Task InitializeAsync()
        {
            if (disposed) throw new Exception(DeviceBase.DeviceDisposedErrorMessage);

            await Task.Run(() =>
            {

                //TODO: Error handling etc.
                UsbDevice.Open();

                //TODO: This is far beyond not cool.
                if (UsbDevice is MonoUsbDevice monoUsbDevice)
                {
                    monoUsbDevice.ClaimInterface(0);
                }
                else if (UsbDevice is WinUsbDevice winUsbDevice)
                {
                    //Doesn't seem necessary in this case...
                }
                else
                {
                    ((IUsbDevice)UsbDevice).ClaimInterface(0);
                }

                _UsbEndpointWriter = UsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
                _UsbEndpointReader = UsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                ReadPacketSize = _UsbEndpointReader.EndpointInfo.Descriptor.MaxPacketSize;

                IsInitialized = true;
            });
        }

        public async Task<byte[]> ReadAsync()
        {
            await _WriteAndReadLock.WaitAsync();

            try
            {
                return await Task.Run(() =>
                {
                    var buffer = new byte[ReadPacketSize];

                    _UsbEndpointReader.Read(buffer, Timeout, out var bytesRead);

                    return buffer;
                });
            }
            finally
            {
                _WriteAndReadLock.Release();
            }
        }

        public async Task WriteAsync(byte[] data)
        {
            await _WriteAndReadLock.WaitAsync();

            try
            {
                await Task.Run(() =>
                {
                    _UsbEndpointWriter.Write(data, Timeout, out var bytesWritten);
                });
            }
            finally
            {
                _WriteAndReadLock.Release();
            }
        }

        public async Task<byte[]> WriteAndReadAsync(byte[] writeBuffer)
        {
            await WriteAsync(writeBuffer);
            return await ReadAsync();
        }
        #endregion

        #region Public Static Methods
        public static int GetVendorId(UsbDevice usbDevice)
        {
            if (usbDevice is MonoUsbDevice monoUsbDevice)
            {
                return monoUsbDevice.Profile.DeviceDescriptor.VendorID;
            }
            else
            {
                return usbDevice.UsbRegistryInfo.Vid;
            }
        }

        public static int GetProductId(UsbDevice usbDevice)
        {
            if (usbDevice is MonoUsbDevice monoUsbDevice)
            {
                return monoUsbDevice.Profile.DeviceDescriptor.ProductID;
            }
            else
            {
                return usbDevice.UsbRegistryInfo.Pid;
            }
        }
        #endregion
    }
}
