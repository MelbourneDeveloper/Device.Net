using Device.Net.Exceptions;
using LibUsbDotNet;
using LibUsbDotNet.LudnMonoLibUsb;
using LibUsbDotNet.Main;
using LibUsbDotNet.WinUsb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.LibUsb
{
    /// <summary>
    /// TODO: Convert this device in to a proper USB device
    /// </summary>
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
        public override ushort WriteBufferSize => throw new NotImplementedException();
        public override ushort ReadBufferSize => throw new NotImplementedException();
        #endregion

        #region Constructor
        public LibUsbDevice(UsbDevice usbDevice, int timeout) : this(usbDevice, timeout, null)
        {
        }

#pragma warning disable CA1062 // Validate arguments of public methods
        public LibUsbDevice(
            UsbDevice usbDevice,
            int timeout,
            ILoggerFactory loggerFactory = null) :
            base(
                usbDevice.DevicePath,
                loggerFactory,
                (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<LibUsbDevice>())
#pragma warning restore CA1062 // Validate arguments of public methods
        {
            UsbDevice = usbDevice ?? throw new ArgumentNullException(nameof(usbDevice));
            Timeout = timeout;
        }
        #endregion

        #region Implementation
        public void Close() => UsbDevice?.Close();

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

                _UsbEndpointWriter = UsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
                _UsbEndpointReader = UsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                ReadPacketSize = _UsbEndpointReader.EndpointInfo.Descriptor.MaxPacketSize;

                _IsInitialized = true;
            });
        }

        public override async Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            await _WriteAndReadLock.WaitAsync(cancellationToken);

            try
            {
                return await Task.Run(() =>
                {
                    var data = new byte[ReadPacketSize];

                    _UsbEndpointReader.Read(data, Timeout, out var bytesRead);

                    Logger.LogTrace(new Trace(false, data));

                    return data;
                }, cancellationToken);
            }
            finally
            {
                _WriteAndReadLock.Release();
            }
        }

        public override async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await _WriteAndReadLock.WaitAsync(cancellationToken);

            try
            {
                await Task.Run(() =>
                {
                    var errorCode = _UsbEndpointWriter.Write(data, Timeout, out var bytesWritten);
                    if (errorCode == ErrorCode.Ok || errorCode == ErrorCode.Success)
                    {
                        Logger.LogTrace(new Trace(true, data));
                    }
                    else
                    {
                        var message = $"Error. Write error code: {errorCode}";
                        Logger?.LogError(message + " {errorCode}", errorCode);
                        throw new IOException(message);
                    }
                }, cancellationToken);
            }
            finally
            {
                _WriteAndReadLock.Release();
            }
        }

        //TODO: make async?
        //TODO: setupPacket not exposed
        public uint SendControlInTransfer(UsbSetupPacket setupPacket)
        {
            var buffer = Array.Empty<byte>();
            UsbDevice.ControlTransfer(ref setupPacket, buffer, buffer.Length, out var length);
            return (uint)length;
        }

        #endregion

        #region Public Static Methods
        public static int GetVendorId(UsbDevice usbDevice)
        {
            return usbDevice == null
                ? throw new ArgumentNullException(nameof(usbDevice))
                : usbDevice is MonoUsbDevice monoUsbDevice ? monoUsbDevice.Profile.DeviceDescriptor.VendorID : usbDevice.UsbRegistryInfo.Vid;
        }

        public static int GetProductId(UsbDevice usbDevice)
        {
            return usbDevice == null
                ? throw new ArgumentNullException(nameof(usbDevice))
                : usbDevice is MonoUsbDevice monoUsbDevice ? monoUsbDevice.Profile.DeviceDescriptor.ProductID : usbDevice.UsbRegistryInfo.Pid;
        }
        #endregion
    }
}
