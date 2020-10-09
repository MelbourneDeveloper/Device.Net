using Android.Content;
using Android.Hardware.Usb;
using Device.Net;
using Flutnet.ServiceModel;
using FlutnetThermometer.ServiceLibrary;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Usb.Net;
using Usb.Net.Android;

namespace FlutnetThermometer.Services
{
    [PlatformService]
    public class ThermometerServiceDroid : ThermometerService
    {
        double _currentTemperature;

        public ThermometerServiceDroid(UsbManager usbManager, Context appContext) : base()
        {
            var filterDeviceDefinitions = new List<FilterDeviceDefinition>
            { new FilterDeviceDefinition ( vendorId : 16701, productId : 8455, usagePage : 65280 ) };

            var deviceDataStreamer =
            filterDeviceDefinitions.CreateAndroidUsbDeviceFactory(usbManager, appContext, writeBufferSize: 8, readBufferSize: 8)
                .ToDeviceManager()
                .CreateDeviceDataStreamer(async (device) =>
                {
                    try
                    {
                        var data = await device.WriteAndReadAsync(new byte[8] { 0x01, 0x80, 0x33, 0x01, 0x00, 0x00, 0x00, 0x00 });

                        var temperatureTimesOneHundred = (data.Data[3] & 0xFF) + (data.Data[2] << 8);

                        _currentTemperature = (double)Math.Round(temperatureTimesOneHundred / 100.0m, 2, MidpointRounding.ToEven);

                    }
                    catch
                    {
                        _currentTemperature = 0;
                    }
                }
                , async (d) =>
                {
                    await d.InitializeAsync();

                    var usbDevice = (IUsbDevice)d;

                    usbDevice.UsbInterfaceManager.WriteUsbInterface = usbDevice.UsbInterfaceManager.UsbInterfaces[1];
                    usbDevice.UsbInterfaceManager.ReadUsbInterface = usbDevice.UsbInterfaceManager.UsbInterfaces[1];
                    usbDevice.UsbInterfaceManager.ReadUsbInterface.ReadEndpoint = usbDevice.UsbInterfaceManager.ReadUsbInterface.UsbInterfaceEndpoints[0];

                }
                ).Start();
        }

        /// <summary>
        /// Specific temperature native method that flutter can call.
        /// </summary>
        /// <returns></returns>
        [PlatformOperation]
        public async override Task<double> GetTemperatureAsync()
        {
            await Task.Delay(1000);
            return _currentTemperature;
        }

    }
}