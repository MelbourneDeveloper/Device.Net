using System;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using usbnet = Usb.Net;

namespace Device.Net.LibUsb
{
    public abstract class LibUsbDeviceFactoryBase : IDeviceFactory
    {
        #region Public Properties
        public ILogger Logger { get; }
        public ITracer Tracer { get; }
        #endregion

        #region Public Abstraction Properties
        public abstract DeviceType DeviceType { get; }
        #endregion

        #region Public Methods
        public async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition)
        {
            return await Task.Run(() =>
            {
                var devices = UsbDevice.AllDevices.ToList();

                if (deviceDefinition == null)

                    return devices.Select(usbRegistry => new ConnectedDeviceDefinition(usbRegistry.DevicePath)
                    {
                        VendorId = (uint)usbRegistry.Vid,
                        ProductId = (uint)usbRegistry.Pid,
                        DeviceType = DeviceType
                    }).ToList();

                if (deviceDefinition.VendorId.HasValue)
                {
                    devices = devices.Where(d => d.Vid == deviceDefinition.VendorId.Value).ToList();
                }

                if (deviceDefinition.ProductId.HasValue)
                {
                    devices = devices.Where(d => d.Pid == deviceDefinition.ProductId.Value).ToList();
                }

                return devices.Select(usbRegistry => new ConnectedDeviceDefinition(usbRegistry.DevicePath) { VendorId = (uint)usbRegistry.Vid, ProductId = (uint)usbRegistry.Pid, DeviceType = DeviceType }).ToList();
            });
        }

        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            if (deviceDefinition == null) throw new ArgumentNullException(nameof(deviceDefinition));
            if (deviceDefinition.VendorId == null) throw new ArgumentNullException(nameof(ConnectedDeviceDefinition.VendorId));
            if (deviceDefinition.ProductId == null) throw new ArgumentNullException(nameof(ConnectedDeviceDefinition.ProductId));

            var usbDeviceFinder = new UsbDeviceFinder((int)deviceDefinition.VendorId.Value, (int)deviceDefinition.ProductId.Value);
            var usbDevice = UsbDevice.OpenUsbDevice(usbDeviceFinder);
            return usbDevice != null ? new usbnet.UsbDevice(usbDevice.DevicePath, new LibUsbInterfaceManager(usbDevice, 3000, Logger, Tracer), Logger, Tracer) : null;
        }
        #endregion

        #region Constructor
        protected LibUsbDeviceFactoryBase(ILogger logger, ITracer tracer)
        {
            Logger = logger;
            Tracer = tracer;
        }
        #endregion
    }
}
