using Hid.Net.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

            deviceObservable.Start();

            var deviceThing = GetDeviceThing(deviceObservable);

            while (true)
            {
                if (deviceThing.Device != null) return;
                await Task.Delay(1000);
            }
        }

        private static IDeviceThing GetDeviceThing(IObservable<ConnectionEventArgs> observable)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IDeviceThing, DeviceObserver>((a) =>
            {
                var deviceObserver = new DeviceObserver();
                observable.Subscribe(deviceObserver);
                return deviceObserver;
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider.GetService<IDeviceThing>();
        }
    }
}
