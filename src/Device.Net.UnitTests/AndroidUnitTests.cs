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
using IusbDevice = Usb.Net.IUsbDevice;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class AndroidUnitTests
    {
        #region Fields
        private const int ExpectedTrezorDataLength = 64;
        private const int TrezorEndpointCount = 2;
        private const int StmDfuEndpointCount = 0;
        private ILoggerFactory loggerFactory;
        private readonly Mock<UsbManager> usbManagerMock = new Mock<UsbManager>();
        private readonly Mock<Context> contextMock = new Mock<Context>();
        private readonly Mock<IAndroidFactory> androidFactoryMock = new Mock<IAndroidFactory>();
        private readonly Mock<UsbDevice> trezorUsbDevice = new Mock<UsbDevice>();
        private readonly Mock<UsbDevice> stmDfuDevice = new Mock<UsbDevice>();
        private readonly Mock<Intent> intentMock = new Mock<Intent>();
        private readonly Mock<UsbDeviceConnection> usbDeviceConnection = new Mock<UsbDeviceConnection>();
        private readonly Mock<UsbInterface> trezorUsbInterfaceMock = new Mock<UsbInterface>();
        private readonly Mock<UsbInterface> stmDfuUsbInterfaceMock = new Mock<UsbInterface>();
        private readonly Mock<UsbEndpoint> firstTrezorEndpointMock = new Mock<UsbEndpoint>();
        private readonly Mock<UsbEndpoint> secondTrezorEndpointMock = new Mock<UsbEndpoint>();
        #endregion

        #region Setup
        [TestInitialize]
        public void Setup()
        {
            loggerFactory = LoggerFactory.Create(builder =>
            {
                _ = builder.AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Trace);
            });

            //Set the return value of the static method
            ByteBuffer.AllocateFunc = new Func<int, ByteBuffer>((c) => new TrezorResponseByteBuffer());
            ByteBuffer.WrapFunc = new Func<byte[]?, ByteBuffer>((c) => new Mock<ByteBuffer>().Object);

            //--------------------Trezor------------------

            //Set up the Trezor usb device
            _ = trezorUsbDevice.Setup(ud => ud.ProductId).Returns(IntegrationTests.TrezorOneProductId);
            _ = trezorUsbDevice.Setup(ud => ud.VendorId).Returns(IntegrationTests.TrezorVendorId);
            //Trezor has one interface
            _ = trezorUsbDevice.Setup(ud => ud.InterfaceCount).Returns(1);
            _ = trezorUsbDevice.Setup(ud => ud.GetInterface(0)).Returns(trezorUsbInterfaceMock.Object);

            //There are 2 endpoints
            _ = trezorUsbInterfaceMock.Setup(ui => ui.EndpointCount).Returns(TrezorEndpointCount);
            _ = trezorUsbInterfaceMock.Setup(ui => ui.GetEndpoint(0)).Returns(firstTrezorEndpointMock.Object);
            _ = trezorUsbInterfaceMock.Setup(ui => ui.GetEndpoint(1)).Returns(secondTrezorEndpointMock.Object);

            //Set up the endpoints
            _ = firstTrezorEndpointMock.Setup(e => e.MaxPacketSize).Returns(64);
            _ = firstTrezorEndpointMock.Setup(e => e.Address).Returns(UsbAddressing.XferInterrupt);
            _ = firstTrezorEndpointMock.Setup(e => e.Direction).Returns(UsbAddressing.In);

            _ = secondTrezorEndpointMock.Setup(e => e.MaxPacketSize).Returns(64);
            _ = secondTrezorEndpointMock.Setup(e => e.Address).Returns(UsbAddressing.XferInterrupt);
            _ = secondTrezorEndpointMock.Setup(e => e.Direction).Returns(UsbAddressing.Out);

            //--------------------STM DFU------------------
            _ = stmDfuDevice.Setup(ud => ud.ProductId).Returns(IntegrationTests.StmDfuProductId);
            _ = stmDfuDevice.Setup(ud => ud.VendorId).Returns(IntegrationTests.StmDfuVendorId);
            //Trezor has one interface
            _ = stmDfuDevice.Setup(ud => ud.InterfaceCount).Returns(1);
            _ = stmDfuDevice.Setup(ud => ud.GetInterface(0)).Returns(stmDfuUsbInterfaceMock.Object);

            //There are 2 endpoints
            _ = stmDfuUsbInterfaceMock.Setup(ui => ui.EndpointCount).Returns(StmDfuEndpointCount);
            //---------------------------------------------

            //The intent should return permission true
            _ = intentMock.Setup(i => i.GetBooleanExtra(It.IsAny<string>(), false)).Returns(true);

            //Allow the interface to be claimed
            _ = usbDeviceConnection.Setup(udc => udc.ClaimInterface(trezorUsbInterfaceMock.Object, true)).Returns(true);
            _ = usbDeviceConnection.Setup(udc => udc.ControlTransferAsync(
                It.IsAny<UsbAddressing>(),
                 It.IsAny<int>(),
                 It.IsAny<int>(),
                 It.IsAny<int>(),
                 It.IsAny<byte[]?>(),
                 It.IsAny<int>(),
                 It.IsAny<int>()))
            .Returns<UsbAddressing, int, int, int, byte[]?, int, int>((a, b, c, d, e, f, g) => Task.FromResult(f));

            //Set up the usb device connection            
            _ = usbManagerMock.Setup(um => um.OpenDevice(trezorUsbDevice.Object)).Returns(usbDeviceConnection.Object);

            //Return a usb request
            _ = androidFactoryMock.Setup(f => f.CreateUsbRequest()).Returns(new Mock<UsbRequest>().Object);

            //Return list of devices including the Trezor
            _ = usbManagerMock.Setup(um => um.DeviceList).Returns(new Dictionary<string, UsbDevice>
            {
                {"asd", trezorUsbDevice.Object },
                {"asd1", stmDfuDevice.Object }
            });
        }
        #endregion

        #region Tests


        [TestMethod]
        public async Task TestWriteAndReadFromTrezorUsbAndroid()
        {
            var device = (IusbDevice)await TestWriteAndReadFromTrezor(
            GetAndroidDeviceFactory(
                new FilterDeviceDefinition(
                vendorId: IntegrationTests.TrezorVendorId,
                productId: IntegrationTests.TrezorOneProductId,
                label: "Trezor One Firmware 1.7.x"), ExpectedTrezorDataLength)
            , ExpectedTrezorDataLength, false
            );

            //Asserts about the device
            Assert.AreEqual(ExpectedTrezorDataLength, device.UsbInterfaceManager.WriteBufferSize);
            Assert.IsNotNull(device.ConnectedDeviceDefinition);
            Assert.AreEqual(IntegrationTests.TrezorOneProductId, (int)device.ConnectedDeviceDefinition.ProductId);
            Assert.AreEqual(IntegrationTests.TrezorVendorId, (int)device.ConnectedDeviceDefinition.VendorId);

            //This is probably not necessary. But, grabbed the device in case we want to do more stuff with it before disposing it
            device.Dispose();

            //Verify that the interface was disposed
            trezorUsbInterfaceMock.Verify(i => i.Dispose(), Times.Once);

            //Verify the interface gets claimed
            usbDeviceConnection.Verify(i => i.ClaimInterface(It.IsAny<UsbInterface>(), It.IsAny<bool>()), Times.Once);

            //Verify we get the exact number of endpoints
            trezorUsbInterfaceMock.Verify(i => i.GetEndpoint(It.IsAny<int>()), Times.Exactly(TrezorEndpointCount));
        }

        [TestMethod]
        public async Task TestSTMDFUModePerformControlTransferSend_DefaultGuid_WinUSBGuid()
        {
            var stmDfuDevice = (IusbDevice)await GetAndroidDeviceFactory(new FilterDeviceDefinition(0x0483, 0xdf11)).ConnectFirstAsync();

            await IntegrationTests.PerformStmDfTest(stmDfuDevice);
        }
        #endregion

        #region Private Methods
        //TODO: there is duplicate code here

        private IDeviceFactory GetAndroidDeviceFactory(FilterDeviceDefinition filterDeviceDefinition, ushort? writeBufferSize = null)
        {
            return filterDeviceDefinition.CreateAndroidUsbDeviceFactory(usbManagerMock.Object,
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
                },
                //TODO: We shouldn't have to specify this. We should pick this up automatically. This is basically a bug on Android
                writeBufferSize: writeBufferSize);

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
        #endregion
    }
}
#endif
