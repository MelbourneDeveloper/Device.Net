Hid devices work slightly differently to USB devices. When you send data (write) to a Hid device, you need to send an [Output Report](https://docs.microsoft.com/en-us/windows-hardware/drivers/hid/sending-hid-reports). You can then read the data as an [Input Report](https://docs.microsoft.com/en-us/windows-hardware/drivers/hid/obtaining-hid-reports). A Report includes data and a Report Id. Different platforms take different approaches in sending/receiving data, but Device.Net puts a layer across this, so you don't need to worry about it. 

## [IHidDevice](https://melbournedeveloper.github.io/Device.Net/api/Hid.Net.IHidDevice.html)
The most straightforward approach is to use the device as an IHidDevice. Cast any Hid device to this interface to get access to the Hid methods. The important methods are `WriteReportAsync` and `ReadReportAsync`. Use the `data` parameter to send the data to the device, and the `ReportId` to specify the report id. The `data` parameter should not include the Report Id because the platform level API will do this for you when writing to the device. If the device expects 64 bytes, you should send 63 bytes and the Report Id separately. 

`ReadReportAsync` reads an input report from the device. You cannot decide on the Report Id. The method picks up whichever input report is in the queue from the device. The `Report` contains the data from the device and the Report Id. You can ignore reports if you are not interested in them. By default, the data will contain the raw data from the device's report with the Report Id separated. If the device sends 64 bytes of data, you will receive 63, and the Report Id is available in the `Report` result.

## Use SendAsync and ReadAsync
If you want to make your code compatible across USB and Hid, you will need to use `SendAsync` and `ReadAsync`. These methods transfer raw data which includes the Report Id. These methods don't have a Report Id as part of the argument or response, so they must be included in the raw data array. For example, if you need to send 64 bytes of data to the device, the actual array you send much be 65, and you must place the Report Id at index 0.

When you receive a [TransferResult](https://melbournedeveloper.github.io/Device.Net/api/Device.Net.TransferResult.html) from ReadAsync, it will include the Report Id at the first byte. This allows you to distinguish the Report Id from the device. So, if you receive 65 bytes, you can discard the first byte when you don't care about the Report Id.

## Transforms
You can apply a transform to the data before it is written to the device, and you can transform the data after it has been read back from the device. This allows you to fix further compatibility issues between Hid and USB. For example, you may have a device that supports both Hid and USB. The USB version expects an array of 64, but the Hid version expects an array of 65 with a Report Id of 0. You can achieve this with a tranform. We usually apply transforms via the factory methods, and they get propagated to the device. For example, you can set the readReportTransform on this [factory method](https://melbournedeveloper.github.io/Device.Net/api/Hid.Net.Windows.WindowsHidDeviceFactoryExtensions.html).

The default transform does this:

```cs
	_readReportTransform = readReportTransform ?? new Func<Report, TransferResult>((readReport) => readReport.ToTransferResult());
```

```cs
public static TransferResult ToTransferResult(this Report readReport)
{
    var rawData = new byte[readReport.TransferResult.Data.Length + 1];

    Array.Copy(readReport.TransferResult.Data, 0, rawData, 1, readReport.TransferResult.Data.Length);

    rawData[0] = readReport.ReportId;

    return new TransferResult(rawData, readReport.TransferResult.BytesTransferred);
}
```

The array is copied to an array which is one byte larger, and the Report Id is set at index 0. You get back an array which contains the data as well as the Report Id at index 0.

The writeTransferTransform works in a similar way. By default, it separates the first byte of the array into the `Report` output report and sends it to the device.

You can see the code in the constructor of [HidDevice](https://github.com/MelbourneDeveloper/Device.Net/blob/develop/src/Hid.Net/HidDevice.cs).

This [example](https://github.com/MelbourneDeveloper/Device.Net/blob/d01cb456438a7622bd581c26af1aa89fc6ab798f/src/Device.Net.UnitTests/IntegrationTests.cs#L164) overrides the conversion of the input report and defaults the output Report Id to zero.