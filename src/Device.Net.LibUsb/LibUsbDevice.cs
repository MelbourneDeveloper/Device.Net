using LibUsbDotNet;
using LibUsbDotNet.LudnMonoLibUsb;
using LibUsbDotNet.Main;
using LibUsbDotNet.WinUsb;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.LibUsb
{
    public class LibUsbDevice : DeviceBase, IDevice
    {
        #region Fields
        private UsbEndpointReader _UsbEndpointReader;
        private UsbEndpointWriter _UsbEndpointWriter;
        private int ReadPacketSize;
        private readonly SemaphoreSlim _WriteAndReadLock = new SemaphoreSlim(1, 1);
        private bool disposed;
        private bool _IsInitialized;
        #endregion

        #region Public Properties
        public UsbDevice UsbDevice { get; }
        public int VendorId => GetVendorId(UsbDevice);
        public int ProductId => GetProductId(UsbDevice);
        public int Timeout { get; }
        public override bool IsInitialized => _IsInitialized;
        public string DeviceId => UsbDevice.DevicePath;
        public override ushort WriteBufferSize => throw new NotImplementedException();
        public override ushort ReadBufferSize => throw new NotImplementedException();
        #endregion

        #region Events
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        #endregion

        #region Constructor
        public LibUsbDevice(UsbDevice usbDevice, int timeout) : this(usbDevice, timeout, null, null)
        {
        }

        public LibUsbDevice(UsbDevice usbDevice, int timeout, ILogger logger, ITracer tracer) : base(logger, tracer)
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

                _IsInitialized = true;
            });
        }

        public override async Task<byte[]> ReadAsync()
        {
            await _WriteAndReadLock.WaitAsync();

            try
            {
                return await Task.Run(() =>
                {
                    var data = new byte[ReadPacketSize];

                    _UsbEndpointReader.Read(data, Timeout, out var bytesRead);

                    Tracer?.Trace(false, data);

                    return data;
                });
            }
            finally
            {
                _WriteAndReadLock.Release();
            }
        }

        public override async Task WriteAsync(byte[] data)
        {
            await _WriteAndReadLock.WaitAsync();

            try
            {
                await Task.Run(() =>
                {
                    var errorCode = _UsbEndpointWriter.Write(data, Timeout, out var bytesWritten);
                    if (errorCode == ErrorCode.Ok || errorCode == ErrorCode.Success)
                    {
                        Tracer?.Trace(true, data);
                    }
                    else
                    {
                        var message = $"Error. Write error code: {errorCode}";
                        Logger?.Log(message, GetType().Name, null, LogLevel.Error);
                        throw new IOException(message);
                    }
                });
            }
            finally
            {
                _WriteAndReadLock.Release();
            }
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
