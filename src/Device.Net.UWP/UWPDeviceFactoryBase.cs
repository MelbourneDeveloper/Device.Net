using Device.Net.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wde = Windows.Devices.Enumeration;

namespace Device.Net.UWP
{
    public abstract class UWPDeviceFactoryBase
    {
        public abstract DeviceType DeviceType { get; }

        protected abstract string GetAqsFilter(uint? vendorId, uint? productId);

        public async Task<IEnumerable<DeviceDefinition>> GetConnectedDeviceDefinitions(uint? vendorId, uint? productId)
        {
            var aqsFilter = GetAqsFilter(vendorId, productId);

            var deviceInformationCollection = await wde.DeviceInformation.FindAllAsync(aqsFilter).AsTask();

            //TODO: Use the properties to fill in the device definition stuff
            //foreach (var deviceInformation in deviceInformationCollection)
            //{
            //    System.Diagnostics.Debug.WriteLine($"{deviceInformation.Id} {string.Join(", ", deviceInformation.Properties.Select(p => p.ToString()))}");
            //}

            return deviceInformationCollection.Select(d => WindowsDeviceFactoryBase.GetDeviceDefinitionFromWindowsDeviceId(d.Id, DeviceType)).ToList();
        }
    }
}
