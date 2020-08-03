using System;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Device.Net.LibUsb
{
    public abstract class LibUsbDeviceFactoryBase : IDeviceFactory
    {
        #region Public Properties
        public ILogger Logger { get; }
        public ITracer Tracer { get; }
        public ILoggerFactory LoggerFactory { get; }
        #endregion

        #region Public Abstraction Properties
        public abstract DeviceType DeviceType { get; }
        #endregion

        #region Public Methods
        public async Task<IEnumerable<ConnectedDeviceDefinition>> GetConnectedDeviceDefinitionsAsync(FilterDeviceDefinition deviceDefinition)
        {
            return await Task.Run(() =>
            {
                IEnumerable<UsbRegistry> devices = UsbDevice.AllDevices;

                if (deviceDefinition == null)
                {
                    return devices.Select(usbRegistry => new ConnectedDeviceDefinition(usbRegistry.DevicePath)
                    {
                        VendorId = (uint)usbRegistry.Vid,
                        ProductId = (uint)usbRegistry.Pid,
                        DeviceType = DeviceType
                    }).ToList();
                }

                if (deviceDefinition.VendorId.HasValue)
                {
                    devices = devices.Where(d => d.Vid == deviceDefinition.VendorId.Value);
                }

                if (deviceDefinition.ProductId.HasValue)
                {
                    devices = devices.Where(d => d.Pid == deviceDefinition.ProductId.Value);
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
            return usbDevice != null ? new LibUsbDevice(usbDevice, 3000, LoggerFactory.CreateLogger(nameof(LibUsbDevice)), Tracer) : null;
        }
        #endregion

        #region Constructor
        protected LibUsbDeviceFactoryBase(ILoggerFactory loggerFactory, ITracer tracer)
        {
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger(nameof(LibUsbDeviceFactoryBase));
            Tracer = tracer;
        }
        #endregion
    }
}
