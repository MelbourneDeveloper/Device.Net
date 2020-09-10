#if !NET45

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Usb.Net.Windows;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class IntegrationTestsUsb
    {


        [TestMethod]
        public async Task TestWriteAndReadFromTrezorUsb()
        {
            var loggerFactory = LoggerFactory.Create((builder) =>
            {
                _ = builder.AddDebug().SetMinimumLevel(LogLevel.Trace);
            });

            var factory = new WindowsUsbDeviceFactory(loggerFactory);
            var deviceManager = new DeviceManager(loggerFactory);
            deviceManager.DeviceFactories.Add(factory);
            var devices = await deviceManager.GetDevicesAsync(new List<FilterDeviceDefinition>
            {
                new FilterDeviceDefinition
                {
                    DeviceType= DeviceType.Usb,
                    VendorId= 0x1209,
                    ProductId=0x53C1,
                    //This does not affect the filtering
                    Label="Trezor One Firmware 1.7.x"
                },
            });

            var trezorDevice = devices.FirstOrDefault();

            Assert.IsNotNull(trezorDevice);
        }

    }
}

#endif
