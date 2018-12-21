using Device.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.HumanInterfaceDevice;
using wde = Windows.Devices.Enumeration;

namespace Hid.Net.UWP
{
    public class UWPHidDeviceFactory : IDeviceFactory
    {
        public DeviceType DeviceType => DeviceType.Hid;

        public static void Register()
        {
            foreach (var deviceFactory in DeviceManager.Current.DeviceFactories)
            {
                if (deviceFactory is UWPHidDeviceFactory) return;
            }

            DeviceManager.Current.DeviceFactories.Add(new UWPHidDeviceFactory());
        }

        public IDevice GetDevice(DeviceDefinition deviceDefinition)
        {
            if (deviceDefinition.DeviceType == DeviceType.Usb) return null;
            return new UWPHidDevice(deviceDefinition.DeviceId);
        }

        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId)
        {
            var aqsFilter = $"System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True AND System.DeviceInterface.Hid.VendorId:={vendorId} AND System.DeviceInterface.Hid.ProductId:={productId} ";

            var deviceInformationCollection = await wde.DeviceInformation.FindAllAsync(aqsFilter).AsTask();

            //foreach (var deviceInformation in deviceInformationCollection)
            //{
            //    System.Diagnostics.Debug.WriteLine($"{deviceInformation.Id} {string.Join(", ", deviceInformation.Properties.Select(p => p.ToString()))}");
            //}

            //TODO: return the vid/pid if we can get it from the properties. Also read/write buffer size

            var deviceDefinitions = deviceInformationCollection.Select(d => new DeviceDefinition { DeviceId = d.Id, DeviceType = DeviceType.Hid }).ToList();
            return deviceDefinitions;
        }
    }
}
