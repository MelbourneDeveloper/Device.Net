using Device.Net.Exceptions;
using LibUsbDotNet;
using LibUsbDotNet.LudnMonoLibUsb;
using LibUsbDotNet.Main;
using LibUsbDotNet.WinUsb;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using usbnet = Usb.Net;

namespace Device.Net.LibUsb
{
    public class LibUsbInterfaceManager : usbnet.UsbInterfaceManager, usbnet.IUsbInterfaceManager
    {
        #region Fields
        private readonly SemaphoreSlim _WriteAndReadLock = new(1, 1);
        private bool disposed;
        private readonly ushort? _writeBufferSize;
        private readonly ushort? _readBufferSize;
        #endregion

        #region Public Properties
        public UsbDevice UsbDevice { get; }
        public int VendorId => GetVendorId(UsbDevice);
        public int ProductId => GetProductId(UsbDevice);
        public int Timeout { get; }
        public bool IsInitialized { get; private set; }
        public ushort WriteBufferSize => WriteUsbInterface.WriteEndpoint.MaxPacketSize;
        public ushort ReadBufferSize => ReadUsbInterface.ReadEndpoint.MaxPacketSize;
        #endregion

        #region Constructor
        public LibUsbInterfaceManager(
            UsbDevice usbDevice,
            int timeout,
            ILoggerFactory loggerFactory,
            ushort? writeBufferSize,
            ushort? readBufferSize) : base(loggerFactory)
        {
            UsbDevice = usbDevice;
            Timeout = timeout;

            _writeBufferSize = writeBufferSize;
            _readBufferSize = readBufferSize;
        }
        #endregion

        #region Protected Overrides
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (disposed) throw new ValidationException(Messages.DeviceDisposedErrorMessage);

            await Task.Run(() =>
            {
                //TODO: Error handling etc.
                _ = UsbDevice.Open();

                switch (UsbDevice)
                {
                    //TODO: This is far beyond not cool.
                    case MonoUsbDevice monoUsbDevice:
                        _ = monoUsbDevice.ClaimInterface(0);
                        break;
                    case WinUsbDevice winUsbDevice:
                        //Doesn't seem necessary in this case...
                        break;
                    default:
                        _ = ((IUsbDevice)UsbDevice).ClaimInterface(0);
                        break;
                }

                foreach (var usbConfigInfo in UsbDevice.Configs)
                {
                    foreach (var usbInterfaceInfo in usbConfigInfo.InterfaceInfoList)
                    {
                        //Create an interface.
                        var usbInterface = new UsbInterface(UsbDevice, usbInterfaceInfo.Descriptor.InterfaceID, null, null, Logger, Timeout);

                        UsbInterfaces.Add(usbInterface);

                        ReadUsbInterface ??= usbInterface;
                        WriteUsbInterface ??= usbInterface;

                        //Write endpoint
                        var usbEndpointWriter = UsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
                        var writeBufferSize = _writeBufferSize ?? 64;
                        var writeEndpoint = new WriteEndpoint(usbEndpointWriter, writeBufferSize);
                        usbInterface.UsbInterfaceEndpoints.Add(writeEndpoint);
                        usbInterface.WriteEndpoint ??= writeEndpoint;

                        //Read endpoint
                        var usbEndpointReader = UsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                        var readBufferSize = _readBufferSize ?? 64;
                        var readEndpoint = new ReadEndpoint(usbEndpointReader, readBufferSize);
                        usbInterface.UsbInterfaceEndpoints.Add(readEndpoint);
                        usbInterface.ReadEndpoint ??= readEndpoint;

                        //int endpointCount = usbInterfaceInfo.EndpointInfoList.Count;

                        /*
                        for (var i = 0; i < endpointCount; i++)
                        {
                            var usbEndpointInfo = usbInterfaceInfo.EndpointInfoList[i];

                            //var IsWrite = (usbEndpointInfo.Descriptor.EndpointID & 128) == 0;
                            //var IsRead = (usbEndpointInfo.Descriptor.EndpointID & 128) != 0;

                            //Write endpoint
                            //var id = usbEndpointInfo.Descriptor.EndpointID ^ 128;
                            //var writeEndpointID = (WriteEndpointID)Enum.Parse(typeof(WriteEndpointID), $"Ep{id.ToString().PadLeft(2, '0')}");
                            var usbEndpointWriter = UsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
                            var writeBufferSize = _WriteBufferSize ?? (ushort)usbEndpointInfo.Descriptor.MaxPacketSize;
                            var writeEndpoint = new WriteEndpoint(usbEndpointWriter, writeBufferSize);
                            usbInterface.UsbInterfaceEndpoints.Add(writeEndpoint);
                            if (usbInterface.WriteEndpoint == null) usbInterface.WriteEndpoint = writeEndpoint;

                            //Read endpoint
                            //var id = usbEndpointInfo.Descriptor.EndpointID ^ 1;
                            //var readEndpointID = (ReadEndpointID)Enum.Parse(typeof(ReadEndpointID), $"Ep{id.ToString().PadLeft(2, '0')}");
                            var usbEndpointReader = UsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                            var readBufferSize = _ReadBufferSize ?? (ushort)usbEndpointInfo.Descriptor.MaxPacketSize;
                            var readEndpoint = new ReadEndpoint(usbEndpointReader, readBufferSize);
                            usbInterface.UsbInterfaceEndpoints.Add(readEndpoint);
                            if (usbInterface.ReadEndpoint == null) usbInterface.ReadEndpoint = readEndpoint;
                        }
                        */
                    }

                    ReadUsbInterface = UsbInterfaces[0];
                    WriteUsbInterface = UsbInterfaces[0];
                }

                IsInitialized = true;
            }, cancellationToken).ConfigureAwait(false);
        }
        #endregion

        #region Implementation
        public override void Close()
        {
            _ = (UsbDevice?.Close());
            base.Close();
        }

        public sealed override void Dispose()
        {
            if (disposed)
            {
                Logger.LogWarning(Messages.WarningMessageAlreadyDisposed, UsbDevice?.DevicePath);
                return;
            }

            disposed = true;

            Logger.LogInformation(Messages.InformationMessageDisposingDevice, UsbDevice?.DevicePath);

            _WriteAndReadLock.Dispose();

            Close();

            base.Dispose();

            GC.SuppressFinalize(this);
        }

        public async Task<TransferResult> ReadAsync()
        {
            await _WriteAndReadLock.WaitAsync().ConfigureAwait(false);

            try
            {
                return await ReadUsbInterface.ReadAsync(ReadBufferSize).ConfigureAwait(false);
            }
            finally
            {
                _ = _WriteAndReadLock.Release();
            }
        }

        public async Task WriteAsync(byte[] data)
        {
            await _WriteAndReadLock.WaitAsync().ConfigureAwait(false);

            try
            {
                _ = await WriteUsbInterface.WriteAsync(data).ConfigureAwait(false);
            }
            finally
            {
                _ = _WriteAndReadLock.Release();
            }
        }

        #endregion

        #region Public Static Methods
        public static int GetVendorId(UsbDevice usbDevice)
            => usbDevice == null ? throw new ArgumentNullException(nameof(usbDevice)) :
            usbDevice is MonoUsbDevice monoUsbDevice ? monoUsbDevice.Profile.DeviceDescriptor.VendorID
            : usbDevice.UsbRegistryInfo.Vid;

        public static int GetProductId(UsbDevice usbDevice)
            => usbDevice == null ? throw new ArgumentNullException(nameof(usbDevice)) :
            usbDevice is MonoUsbDevice monoUsbDevice ? monoUsbDevice.Profile.DeviceDescriptor.ProductID
            : usbDevice.UsbRegistryInfo.Pid;

        public Task<ConnectedDeviceDefinition> GetConnectedDeviceDefinitionAsync(CancellationToken cancellationToken = default)
        {
            //TODO: this isn't very nice

            var usbRegistryInfo = UsbDevice.UsbRegistryInfo;

            if (usbRegistryInfo == null)
            {
                return Task.FromResult(new ConnectedDeviceDefinition(
                    UsbDevice.DevicePath,
                    DeviceType.Usb,
                    (uint)GetVendorId(UsbDevice),
                    (uint)GetProductId(UsbDevice)));
            }

            var result = usbRegistryInfo.ToConnectedDevice();
            return Task.FromResult(result);

            //TODO: Return more information
        }
        #endregion
    }
}
