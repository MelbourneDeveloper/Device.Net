#if !WINDOWS_UWP && !NET45

using Android.Content;
using Android.Hardware.Usb;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Usb.Net.Android;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class AndroidUnitTests
    {
        private async Task TestWriteAndReadFromTrezor(IDeviceFactory deviceFactory, int expectedDataLength = 64)
        {
            //Send the request part of the Message Contract
            var request = new byte[64];
            request[0] = 0x3f;
            request[1] = 0x23;
            request[2] = 0x23;

            var integrationTester = new IntegrationTester(
                deviceFactory);
            await integrationTester.TestAsync(request, IntegrationTests.AssertTrezorResult, expectedDataLength);
        }

        [TestMethod]
        public async Task TestWriteAndReadFromTrezorUsbAndroid()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
             {
                 _ = builder.AddConsole()
                 .AddDebug()
                 .SetMinimumLevel(LogLevel.Trace);
             });

            var usbManagerMock = new Mock<UsbManager>();
            var contextMock = new Mock<Context>();
            var androidFactoryMock = new Mock<IAndroidFactory>();
            var usbDevice = new Mock<UsbDevice>();
            var intentMock = new Mock<Intent>();
            var usbDeviceConnection = new Mock<UsbDeviceConnection>();

            //Set up the Trezor usb device as being connected
            _ = usbDevice.Setup(ud => ud.ProductId).Returns(IntegrationTests.TrezorOneProductId);
            _ = usbDevice.Setup(ud => ud.VendorId).Returns(IntegrationTests.TrezorVendorId);
            _ = usbManagerMock.Setup(um => um.DeviceList).Returns(new Dictionary<string, UsbDevice>
            {
                {"asd", usbDevice.Object }
            });

            //The intent should return permission true
            _ = intentMock.Setup(i => i.GetBooleanExtra(It.IsAny<string>(), false)).Returns(true);

            //Set up the usb device connection            
            _ = usbManagerMock.Setup(um => um.OpenDevice(usbDevice.Object)).Returns(usbDeviceConnection.Object);

            await TestWriteAndReadFromTrezor(
            new FilterDeviceDefinition(
                vendorId: IntegrationTests.TrezorVendorId,
                productId: IntegrationTests.TrezorOneProductId,
                label: "Trezor One Firmware 1.7.x")
            .CreateAndroidUsbDeviceFactory(usbManagerMock.Object,
                contextMock.Object,
                loggerFactory,
                androidFactory: androidFactoryMock.Object,
                getUsbPermissionBroadcastReceiver: (ud)
                =>
                {
                    //Why do we have to do this?

                    //We return the receiver but...
                    var usbPermissionBroadcastReceiver = new UsbPermissionBroadcastReceiver(usbManagerMock.Object, ud, contextMock.Object, androidFactoryMock.Object);

                    //We run this and send a receive until the received event fires
                    FakeReceiveAsync(usbPermissionBroadcastReceiver);

                    return usbPermissionBroadcastReceiver;
                })
            );

            ///Keeps running until the received event fires which allows code elsewhere to continue
            async Task FakeReceiveAsync(UsbPermissionBroadcastReceiver usbPermissionBroadcastReceiver)
            {
                var received = false;
                usbPermissionBroadcastReceiver.Received += (s, e) => { received = true; };
                while (!received)
                {
                    await Task.Delay(10);
                    usbPermissionBroadcastReceiver.OnReceive(contextMock.Object, intentMock.Object);
                }
            }
        }
    }
}
#endif
