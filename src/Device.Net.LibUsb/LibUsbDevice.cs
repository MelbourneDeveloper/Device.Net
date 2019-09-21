using Device.Net.Exceptions;
using LibUsbDotNet;
using LibUsbDotNet.LudnMonoLibUsb;
using LibUsbDotNet.Main;
using LibUsbDotNet.WinUsb;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using usbnet = Usb.Net;

namespace Device.Net.LibUsb
{
    public class LibUsbInterfaceManager : usbnet.UsbInterfaceManager, usbnet.IUsbInterfaceManager
    {
        #region Fields
        private UsbEndpointReader _UsbEndpointReader;
        private UsbEndpointWriter _UsbEndpointWriter;
        private int ReadPacketSize;
        private readonly SemaphoreSlim _WriteAndReadLock = new SemaphoreSlim(1, 1);
        private bool disposed;
        #endregion

        #region Public Properties
        public UsbDevice UsbDevice { get; }
        public int VendorId => GetVendorId(UsbDevice);
        public int ProductId => GetProductId(UsbDevice);
        public int Timeout { get; }
        public bool IsInitialized { get; private set; }
        public ushort WriteBufferSize { get; }
        public ushort ReadBufferSize { get; }
        #endregion

        #region Constructor
        public LibUsbInterfaceManager(UsbDevice usbDevice, int timeout, ILogger logger, ITracer tracer, ushort writeBufferSize, ushort readBufferSize) : base(logger, tracer)
        {
            UsbDevice = usbDevice;
            Timeout = timeout;

            WriteBufferSize = writeBufferSize;
            ReadBufferSize = readBufferSize;
        }
        #endregion

        #region Implementation
        public void Close()
        {
            UsbDevice?.Close();
        }

        public override void Dispose()
        {
            if (disposed) return;
            disposed = true;

            _WriteAndReadLock.Dispose();

            Close();

            base.Dispose();

            GC.SuppressFinalize(this);
        }

        public async Task InitializeAsync()
        {
            if (disposed) throw new ValidationException(Messages.DeviceDisposedErrorMessage);

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

                var dummyInterface = new DummyInterface(Logger, Tracer, ReadBufferSize, WriteBufferSize);

                _UsbEndpointWriter = UsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);

                _UsbEndpointReader = UsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                var writeEndpoint = new WriteEndpoint(_UsbEndpointWriter, (ushort)ReadPacketSize);

                var readEndpoint = new ReadEndpoint(_UsbEndpointReader, (ushort)ReadPacketSize);


                this.UsbInterfaces.Add(dummyInterface);

                ReadPacketSize = _UsbEndpointReader.EndpointInfo.Descriptor.MaxPacketSize;

                IsInitialized = true;
            });
        }

        public async Task<ReadResult> ReadAsync()
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

        public async Task WriteAsync(byte[] data)
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

        public Task<ConnectedDeviceDefinitionBase> GetConnectedDeviceDefinitionAsync()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
