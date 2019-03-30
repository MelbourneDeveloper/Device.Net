using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net.LibUsb
{
    public abstract class LibUsbDeviceFactoryBase : IDeviceFactory
    {
        #region Public Properties
        public ILogger Logger { get; set; }
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

                if (deviceDefinition != null)
                {
                    if (deviceDefinition.VendorId.HasValue)
                    {
                        devices = devices.Where(d => d.Vid == deviceDefinition.VendorId.Value);
                    }

                    if (deviceDefinition.VendorId.HasValue)
                    {
                        devices = devices.Where(d => d.Pid == deviceDefinition.ProductId.Value);
                    }
                }

                var retVal = new List<ConnectedDeviceDefinition>();

                foreach (var usbRegistry in devices)
                {
                    retVal.Add(new ConnectedDeviceDefinition(usbRegistry.DevicePath)
                    {
                        VendorId = (uint)usbRegistry.Vid,
                        ProductId = (uint)usbRegistry.Pid,
                        DeviceType = DeviceType
                    });
                }

                return retVal;
            });
        }

        public IDevice GetDevice(ConnectedDeviceDefinition deviceDefinition)
        {
            var usbDeviceFinder = new UsbDeviceFinder((int)deviceDefinition.VendorId.Value, (int)deviceDefinition.ProductId.Value);
            var usbDevice = UsbDevice.OpenUsbDevice(usbDeviceFinder);
            return usbDevice != null ? new LibUsbDevice(usbDevice, 3000) { Logger = Logger } : null;
        }
        #endregion
    }
}
