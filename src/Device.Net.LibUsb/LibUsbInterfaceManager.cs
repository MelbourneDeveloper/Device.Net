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
        private readonly SemaphoreSlim _WriteAndReadLock = new SemaphoreSlim(1, 1);
        private bool disposed;

        private ushort? _writeBufferSize { get; }
        private ushort? _readBufferSize { get; }
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
        public LibUsbInterfaceManager(UsbDevice usbDevice, int timeout, ILogger logger, ushort? writeBufferSize, ushort? readBufferSize) : base(logger, tracer)
        {
            UsbDevice = usbDevice;
            Timeout = timeout;

            _writeBufferSize = writeBufferSize;
            _readBufferSize = readBufferSize;
        }
        #endregion

        #region Protected Overrides
        protected override async Task InitializeProtectedAsync()
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

                foreach (var usbConfigInfo in UsbDevice.Configs)
                {
                    foreach (var usbInterfaceInfo in usbConfigInfo.InterfaceInfoList)
                    {
                        //Create an interface.
                        var usbInterface = new UsbInterface(Timeout, Tracer, null, Logger, null, usbInterfaceInfo.Descriptor.InterfaceID);

                        UsbInterfaces.Add(usbInterface);

                        if (ReadUsbInterface == null) ReadUsbInterface = usbInterface;
                        if (WriteUsbInterface == null) WriteUsbInterface = usbInterface;

                        //Write endpoint
                        var usbEndpointWriter = UsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
                        var writeBufferSize = _writeBufferSize ?? (ushort)64;
                        var writeEndpoint = new WriteEndpoint(usbEndpointWriter, writeBufferSize);
                        usbInterface.UsbInterfaceEndpoints.Add(writeEndpoint);
                        if (usbInterface.WriteEndpoint == null) usbInterface.WriteEndpoint = writeEndpoint;

                        //Read endpoint
                        var usbEndpointReader = UsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                        var readBufferSize = _readBufferSize ?? (ushort)64;
                        var readEndpoint = new ReadEndpoint(usbEndpointReader, readBufferSize);
                        usbInterface.UsbInterfaceEndpoints.Add(readEndpoint);
                        if (usbInterface.ReadEndpoint == null) usbInterface.ReadEndpoint = readEndpoint;

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
            });
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

        public async Task<TransferResult> ReadAsync()
        {
            await _WriteAndReadLock.WaitAsync();

            try
            {
                return await ReadUsbInterface.ReadAsync(ReadBufferSize);
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
                await WriteUsbInterface.WriteAsync(data);
            }
            finally
            {
                _WriteAndReadLock.Release();
            }
        }

        #endregion

        #region Public Static Methods
        public static int GetVendorId(UsbDevice usbDevice) => usbDevice is MonoUsbDevice monoUsbDevice ? monoUsbDevice.Profile.DeviceDescriptor.VendorID : usbDevice.UsbRegistryInfo.Vid;

        public static int GetProductId(UsbDevice usbDevice) => usbDevice is MonoUsbDevice monoUsbDevice ? monoUsbDevice.Profile.DeviceDescriptor.ProductID : usbDevice.UsbRegistryInfo.Pid;

        public Task<ConnectedDeviceDefinition> GetConnectedDeviceDefinitionAsync()
        {
            //TODO: this isn't very nice

            var usbRegistryInfo = UsbDevice.UsbRegistryInfo;
            if (usbRegistryInfo != null)
            {
                var result = usbRegistryInfo.ToConnectedDevice();
                return Task.FromResult(result);
            }

            return Task.FromResult(new ConnectedDeviceDefinition(UsbDevice.DevicePath) { });
        }
        #endregion
    }
}
