using Device.Net;
using Device.Net.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wde = Windows.Devices.Enumeration;

namespace Usb.Net.UWP
{
    public class UWPUsbDeviceFactory : IDeviceFactory
    {
        public DeviceType DeviceType => DeviceType.Usb;

        public static void Register()
        {
            foreach (var deviceFactory in DeviceManager.Current.DeviceFactories)
            {
                if (deviceFactory is UWPUsbDeviceFactory) return;
            }

            DeviceManager.Current.DeviceFactories.Add(new UWPUsbDeviceFactory());
        }

        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            if (deviceDefinition.DeviceType == DeviceType.Hid) return null;
            return new UWPUsbDevice(deviceDefinition.DeviceId);
        }

        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId)
        {
            var aqsFilter = "System.Devices.InterfaceClassGuid:=\"{DEE824EF-729B-4A0E-9C14-B7117D33A817}\" AND System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True AND " + $" System.DeviceInterface.WinUsb.UsbVendorId:={vendorId.Value} AND System.DeviceInterface.WinUsb.UsbProductId:={productId.Value}";

            var deviceInformationCollection = await wde.DeviceInformation.FindAllAsync(aqsFilter).AsTask();

            //TODO: return the vid/pid if we can get it from the properties. Also read/write buffer size

            var deviceIds = deviceInformationCollection.Select(d => WindowsDeviceFactoryBase.GetDeviceDefinitionFromWindowsDeviceId(d.Id, DeviceType.Usb)).ToList();
            return deviceIds;
        }
    }
}
