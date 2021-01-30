I'm keen to help you integrate your USB, HID, and other devices with computers, tablets, and phones. I can help you [build cross-platform apps](https://christianfindlay.com/apps/) to communicate with your device or firmware. I hope that you will choose Device.Net as the framework for integrating your connected device with a host. If you manufacture devices or develop firmware for device boards, I would like to offer you some free time. Hit me up on [LinkedIn](https://www.linkedin.com/in/christian-findlay/) or [Twitter](https://twitter.com/CFDevelop). You will need to send me one of your devices, and we can negotiate some of my time as a starting point.

Device.Net has been tested with several USB and HID devices, so most of the functionality you will need already exists. However, if you have issues with getting something to work, there are some steps you need to take. Firstly, I ask that you clone the repo and modify one of the samples or unit tests to meet your needs. If that step fails, please follow on.

## Specify The Operation
You need to specify the _**operation**_ that you are trying to achieve. For example, the operation might be to turn on a light connected to the device. It could turn the volume up or down, upgrade the firmware, or calculate the decimal places of pi. You need to specify what you are trying to achieve by connecting to the device.

## Define Your Message Contract
A Message Contract contains a request to the device and a response from the device. The contract reflects the operation you need to perform. Device.Net sends the request from the host to the device and receives the expected response. This should perform the operation on the device. Both the request and response are arrays of bytes (`byte[]`). This is the fundamental requirement to move forward. On your side, the device will receive some data, process it, and then send it back as the response to Device.Net. The contract specifies that given request _x_, the device will respond with response _y_. Use a tool like [usblyzer](http://www.usblyzer.com/misprints.htm) to capture the traffic between your device and the host.

The response part must be a valid, thoroughly tested response from the device on your side. You must know that the device responds with the expected result when it receives the given request.

_Note: if the protocol is USB, you need to specify which interface and pipe address to use. For example, does it use Bulk, Control Transfer, or interrupt?_

> But, my device doesn't always send back the same response... 

That's fine. Just allow for this in the integration test. 

## Write a Failing Integration Test
As a starting point, you need to define a contract and write it into a failing Integration Test.  Here is an [example](https://github.com/MelbourneDeveloper/Device.Net/blob/d01cb456438a7622bd581c26af1aa89fc6ab798f/src/Device.Net.UnitTests/IntegrationTests.cs#L252). The request only has five bytes of data, but the response verifies several properties about the response and the device.

```cs
[TestMethod]
public async Task TestWriteAndReadFromNanoHid()
{
    //Send the request part of the Message Contract
    var request = new byte[NanoBufferSize];
    request[0] = 63;
    request[1] = 62;
    request[2] = 1;
    request[3] = 1;
    request[4] = 1;

    var filterDeviceDefinition = new FilterDeviceDefinition(productId: 4112, vendorId: 10741);

    var integrationTester = new IntegrationTester(filterDeviceDefinition.GetHidDeviceFactory(loggerFactory));

    await integrationTester.TestAsync(request, (result, device) =>
        {
            Assert.AreEqual(NanoBufferSize, result.Data.Length);
            Assert.AreEqual(63, result.Data[0]);
            Assert.AreEqual(62, result.Data[1]);

            var windowsHidDevice = (HidDevice)device;

            //TODO: share this with UWP
            Assert.AreEqual(DeviceType.Hid, device.ConnectedDeviceDefinition.DeviceType);
            Assert.AreEqual("AirNetix", device.ConnectedDeviceDefinition.Manufacturer);
            Assert.AreEqual(filterDeviceDefinition.ProductId, device.ConnectedDeviceDefinition.ProductId);
            Assert.AreEqual(filterDeviceDefinition.VendorId, device.ConnectedDeviceDefinition.VendorId);
            Assert.AreEqual("STS-170", device.ConnectedDeviceDefinition.ProductName);
            Assert.AreEqual(NanoBufferSize, device.ConnectedDeviceDefinition.ReadBufferSize);
            Assert.AreEqual(NanoBufferSize, device.ConnectedDeviceDefinition.WriteBufferSize);
            Assert.AreEqual("000000000001", device.ConnectedDeviceDefinition.SerialNumber);
            Assert.AreEqual((ushort)1, device.ConnectedDeviceDefinition.Usage);
            Assert.AreEqual((ushort)65280, device.ConnectedDeviceDefinition.UsagePage);
            Assert.AreEqual((ushort)256, device.ConnectedDeviceDefinition.VersionNumber);
            Assert.AreEqual(NanoBufferSize, windowsHidDevice.ReadBufferSize);
            Assert.AreEqual(NanoBufferSize, windowsHidDevice.WriteBufferSize);
            return Task.FromResult(true);

        }, NanoBufferSize);
}
```

In the example above, the test will fail if any of the data does not match the contract's response part. You may need to make allowances for values that may change on each call.

Once I have this code and your device, I have what I need to investigate issues, but without this, I cannot move forward. 

_Note: the test does not need to compile. If you need to add pretend APIs that are missing. That is fine._

## Send Your Device
Please send your device via a shipping service that delivers to post office boxes. I will send you the details of that for shipping purposes.

I normally work with devices that already have firmware installed. I expect that I can plug in the device and run your integration test. The integration test should fail so I can see what is going wrong. This is the optimal scenario, and I will spend some time trying to fix the issues that I see.

## Devices Without Firmware

Sometimes, your device may not come with preloaded firmware. This is OK, but please refer back to the Message Contract as the starting point. I need an integration test with a defined message contract. I can install the firmware on your device, but I need confirmation that it complies with the message contract on your side. When Device.Net sends the request part of the contract to the firmware, the firmware needs to respond with the expected result. 

Start with the Message Contract and work backward to the firmware. Have you clearly defined the contract? Does the firmware respond with the expected response when it receives the given request? If the answer is yes on both accounts, I will install the firmware with your instructions.

Lastly, please understand that I don't know anything about microcontrollers (MCU) or DFU, and I had to Google these terms. Please don't assume that I know anything about the firmware side of USB connectivity. If the instructions are not crystal clear and easily replicable, I will quickly burn the free time negotiated. 

