#if !WINDOWS_UWP && !NET45

using Android.Content;
using Android.Hardware.Usb;
using Java.Nio;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Usb.Net.Android;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class AndroidUnitTests
    {
        private Task<IDevice> TestWriteAndReadFromTrezor(IDeviceFactory deviceFactory, int expectedDataLength = 64, bool dispose = true)
        {
            //Send the request part of the Message Contract
            var integrationTester = new IntegrationTester(
                deviceFactory);

            return integrationTester.TestAsync(GetTrezorRequest(), IntegrationTests.AssertTrezorResult, expectedDataLength, dispose);
        }

        private static byte[] GetTrezorRequest()
        {
            var request = new byte[64];
            request[0] = 0x3f;
            request[1] = 0x23;
            request[2] = 0x23;
            return request;
        }

        [TestMethod]
        public async Task TestWriteAndReadFromTrezorUsbAndroid()
        {
            const int ExpectedDataLength = 64;

            var loggerFactory = LoggerFactory.Create(builder =>
             {
                 _ = builder.AddConsole()
                 .AddDebug()
                 .SetMinimumLevel(LogLevel.Trace);
             });

            //Set the return value of the static method
            ByteBuffer.AllocateFunc = new Func<int, ByteBuffer>((c) => new TrezorResponseByteBuffer());
            ByteBuffer.WrapFunc = new Func<byte[]?, ByteBuffer>((c) => new Mock<ByteBuffer>().Object);

            var usbManagerMock = new Mock<UsbManager>();
            var contextMock = new Mock<Context>();
            var androidFactoryMock = new Mock<IAndroidFactory>();
            var usbDevice = new Mock<UsbDevice>();
            var intentMock = new Mock<Intent>();
            var usbDeviceConnection = new Mock<UsbDeviceConnection>();
            var usbInterfaceMock = new Mock<UsbInterface>();
            var firstEndpointMock = new Mock<UsbEndpoint>();
            var secondEndpointMock = new Mock<UsbEndpoint>();

            //Set up the Trezor usb device
            _ = usbDevice.Setup(ud => ud.ProductId).Returns(IntegrationTests.TrezorOneProductId);
            _ = usbDevice.Setup(ud => ud.VendorId).Returns(IntegrationTests.TrezorVendorId);
            //Trezor has one interface
            _ = usbDevice.Setup(ud => ud.InterfaceCount).Returns(1);
            _ = usbDevice.Setup(ud => ud.GetInterface(0)).Returns(usbInterfaceMock.Object);

            //There are 2 endpoints
            _ = usbInterfaceMock.Setup(ui => ui.EndpointCount).Returns(2);
            _ = usbInterfaceMock.Setup(ui => ui.GetEndpoint(0)).Returns(firstEndpointMock.Object);
            _ = usbInterfaceMock.Setup(ui => ui.GetEndpoint(1)).Returns(secondEndpointMock.Object);

            //Return list of devices including the Trezor
            _ = usbManagerMock.Setup(um => um.DeviceList).Returns(new Dictionary<string, UsbDevice>
            {
                {"asd", usbDevice.Object }
            });

            //Allow the interface to be claimed
            _ = usbDeviceConnection.Setup(udc => udc.ClaimInterface(usbInterfaceMock.Object, true)).Returns(true);

            //The intent should return permission true
            _ = intentMock.Setup(i => i.GetBooleanExtra(It.IsAny<string>(), false)).Returns(true);

            //Set up the usb device connection            
            _ = usbManagerMock.Setup(um => um.OpenDevice(usbDevice.Object)).Returns(usbDeviceConnection.Object);

            //Set up the endpoints
            _ = firstEndpointMock.Setup(e => e.MaxPacketSize).Returns(64);
            _ = firstEndpointMock.Setup(e => e.Address).Returns(UsbAddressing.XferInterrupt);
            _ = firstEndpointMock.Setup(e => e.Direction).Returns(UsbAddressing.In);

            _ = secondEndpointMock.Setup(e => e.MaxPacketSize).Returns(64);
            _ = secondEndpointMock.Setup(e => e.Address).Returns(UsbAddressing.XferInterrupt);
            _ = secondEndpointMock.Setup(e => e.Direction).Returns(UsbAddressing.Out);

            //Return a usb request
            _ = androidFactoryMock.Setup(f => f.CreateUsbRequest()).Returns(new Mock<UsbRequest>().Object);

            var device = await TestWriteAndReadFromTrezor(
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
                }), ExpectedDataLength, false
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

            //This is probably not necessary. But, grabbed the device in case we want to do more stuff with it before disposing it
            device.Dispose();

            //Verify that the interface was disposed
            usbInterfaceMock.Verify(i => i.Dispose(), Times.Once);
        }
    }
}
#endif
