Android and UWP apps both require that you specify which devices you will need to access in their manifest files. This is because Store apps are secure and won't let a given app access devices without the user's knowledge. On UWP, if you do not specify the setup correctly, you will be able to enumerate the device, but not connect to it. 

Here is an example Capabilities section of the Package.appxmanifest for Hid and USB on UWP:

```xml
  <Capabilities>
    <Capability Name="internetClient" />

    <uap:Capability Name="removableStorage" />

    <DeviceCapability Name="humaninterfacedevice">

      <Device Id="vidpid:534C 0001">
        <Function Type="usage:0005 *" />
        <Function Type="usage:FF00 0001" />
        <Function Type="usage:ff00 *" />
      </Device>

      <Device Id="vidpid:1209 53C0">
        <Function Type="usage:0005 *" />
        <Function Type="usage:FF00 0001" />
        <Function Type="usage:ff00 *" />
      </Device>

      <Device Id="vidpid:1209 53C1">
        <Function Type="usage:0005 *" />
        <Function Type="usage:FF00 0001" />
        <Function Type="usage:ff00 *" />
      </Device>

    </DeviceCapability>

    <DeviceCapability Name="usb">

      <!--Trezor Firmware 1.7.x -->
      <Device Id="vidpid:1209 53C1">
        <Function Type="classId:ff * *" />
      </Device>

    </DeviceCapability>

  </Capabilities>
```
[Code Reference](https://github.com/MelbourneDeveloper/Device.Net/blob/b703a5eb5576c06ddb6ab9b9412615b75c792c66/src/Usb.Net.UWP.Sample/Package.appxmanifest#L46)

Here is an Android example of the device_filter.xml file on Android (This is a resource file found at Resources/xml). Note these are the same devices as above written in decimal instead of hex.

```xml
<?xml version="1.0" encoding="utf-8"?>
<resources>
  <usb-device vendor-id="21324" product-id="1" />
  <usb-device vendor-id="4617" product-id="21440" />
  <usb-device vendor-id="4617" product-id="21441" />
</resources>
```

[Code Reference](https://github.com/MelbourneDeveloper/Trezor.Net/blob/a964eeddbd68dc57c4b81fbe28ae6444b0137a0b/src/Trezor.Net.XamarinFormsSample/XFAS/Resources/xml/device_filter.xml#L1)