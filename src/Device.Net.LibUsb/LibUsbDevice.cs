using LibUsbDotNet;
using LibUsbDotNet.LudnMonoLibUsb;
using LibUsbDotNet.Main;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.LibUsb.MacOS
{
    public class LibUsbDevice : IDevice
    {
        #region Fields
        private UsbEndpointReader _UsbEndpointReader;
        private UsbEndpointWriter _UsbEndpointWriter;
        private int ReadPacketSize;
        private SemaphoreSlim _WriteAndReadLock = new SemaphoreSlim(1, 1);
        #endregion

        #region Public Properties
        public UsbDevice UsbDevice { get; }
        public int VendorId => UsbDevice.UsbRegistryInfo.Vid;
        public int ProductId => UsbDevice.UsbRegistryInfo.Pid;
        public int Timeout { get; }
        public bool IsInitialized => true;
        public ConnectedDeviceDefinitionBase ConnectedDeviceDefinition => throw new NotImplementedException();
        public string DeviceId => throw new NotImplementedException();
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
        public void Dispose()
        {
            //TODO: Release the device...
            // UsbDevice.Dispose();
        }


        public async Task InitializeAsync()
        {
            await Task.Run(() =>
            {

                //TODO: Error handling etc.
                UsbDevice.Open();

                //TODO: This is far beyond not cool.
                if (UsbDevice is MonoUsbDevice monoUsbDevice)
                {
                    monoUsbDevice.ClaimInterface(0);
                }
                else
                {
                    ((IUsbDevice)UsbDevice).ClaimInterface(0);
                }

                _UsbEndpointWriter = UsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
                _UsbEndpointReader = UsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                ReadPacketSize = _UsbEndpointReader.EndpointInfo.Descriptor.MaxPacketSize;
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
    }
}
