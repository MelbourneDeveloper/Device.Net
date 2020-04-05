using Hid.Net.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Usb.Net.Windows;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class DeviceObservableTests
    {
        [TestMethod]
        public async Task TestPubSub()
        {
            var deviceObservable = new DeviceMonitor(
                new DeviceManager(),
                new List<FilterDeviceDefinition>
                {
                    new FilterDeviceDefinition{ DeviceType= DeviceType.Hid, VendorId= 0x534C, ProductId=0x0001, Label="Trezor One Firmware 1.6.x", UsagePage=65280 },
                    new FilterDeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x534C, ProductId=0x0001, Label="Trezor One Firmware 1.6.x (Android Only)" },
                    new FilterDeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x1209, ProductId=0x53C1, Label="Trezor One Firmware 1.7.x" },
                    new FilterDeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x1209, ProductId=0x53C0, Label="Model T" }
                });

            deviceObservable.DeviceManager.DeviceFactories.Add(new WindowsUsbDeviceFactory(null, null));
            deviceObservable.DeviceManager.DeviceFactories.Add(new WindowsHidDeviceFactory(null, null));

            var deviceObserver = new DeviceObserver();

            deviceObservable.Subscribe(deviceObserver);

            deviceObservable.Start();

            var hasConnected = false;

            deviceObserver.ConnectionEventOccurred += (sender, args) => { if (!args.IsDisconnection) hasConnected = true; };

            while (!hasConnected)
            {
                await Task.Delay(1000);
            }
        }
    }
}
