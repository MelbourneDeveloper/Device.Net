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
As a starting point, you need to define a contract and write it into a failing Integration Test.  Here is an [example](https://github.com/MelbourneDeveloper/Device.Net/blob/cc28e96559214456a2a0bdbe401019a69dd0a825/src/Device.Net.UnitTests/IntegrationTestsUsb.cs#L16) from the Trezor hardwarewallet. The request only has three bytes of data, but the response is a full array of 64 bytes.

```cs
[TestMethod]
public async Task TestWriteAndReadFromTrezorUsb()
{
    //Note: creating a LoggerFactory like this is easier than creating a mock
    var loggerFactory = LoggerFactory.Create((builder) =>
    {
        _ = builder.AddDebug().SetMinimumLevel(LogLevel.Trace);
    });

    var factory = new WindowsUsbDeviceFactory(loggerFactory);
    var deviceManager = new DeviceManager(loggerFactory);
    deviceManager.DeviceFactories.Add(factory);

    //Get the filtered devices
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

    //Get the first available device
    var trezorDevice = devices.FirstOrDefault();

    //Ensure that it gets picked up
    Assert.IsNotNull(trezorDevice);

    //Initialize the device
    await trezorDevice.InitializeAsync();

    //Send the request part of the Message Contract
    var request = new byte[64];
    request[0] = 0x3f;
    request[1] = 0x23;
    request[2] = 0x23;
    var response = await trezorDevice.WriteAndReadAsync(request);

    //Specify the response part of the Message Contract
    var expectedResult = new byte[] { 63, 35, 35, 0, 17, 0, 0, 0, 194, 10, 9, 116, 114, 101, 122, 111, 114, 46, 105, 111, 16, 1, 24, 9, 32, 1, 50, 24, 51, 66, 69, 65, 55, 66, 50, 55, 50, 55, 66, 49, 55, 57, 50, 52, 67, 56, 67, 70, 68, 56, 53, 48, 56, 1, 64, 0, 74, 5, 101, 110, 45, 85, 83, 82 };

    //Assert that the response part meets the specification
    Assert.IsTrue(expectedResult.SequenceEqual(response.Data));
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

