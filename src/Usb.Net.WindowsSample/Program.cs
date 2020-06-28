using System.Threading;
using System.Threading.Tasks;
using Device.Net;
using System.Collections.Generic;
using System.Linq;
using System;

#if (!LIBUSB)
using Usb.Net.Windows;
using Hid.Net.Windows;
using SerialPort.Net.Windows;
#else
using Device.Net.LibUsb;
#endif

namespace Usb.Net.WindowsSample
{
    internal class Program
    {
        #region Fields
        private static readonly IDeviceManager _DeviceManager = new DeviceManager();
        /// <summary>
        /// TODO: Test these!
        /// </summary>
        private static readonly DebugLogger Logger = new DebugLogger();
        private static readonly DebugTracer Tracer = new DebugTracer();
        #endregion

        #region Main
        private static void Main(string[] args)
        {
            //Register the factories for creating Usb devices. This only needs to be done once.
#if (LIBUSB)
            _DeviceManager.RegisterDeviceFactory(new LibUsbUsbDeviceFactory(Logger, Tracer));
#else
            _DeviceManager.RegisterDeviceFactory(new WindowsHidDeviceFactory(Logger, Tracer));
#endif
            GoAsync();

            new ManualResetEvent(false).WaitOne();
        }

        private static async Task GoAsync()
        {
            //Thanks to https://github.com/WozSoftware
            //https://github.com/WozSoftware/Woz.TEMPer/blob/dcd0b49d67ac39d10c3759519050915816c2cd93/Woz.TEMPer/Sensors/TEMPerV14.cs#L15

            var devices = (await _DeviceManager.GetDevicesAsync(new List<FilterDeviceDefinition> { new FilterDeviceDefinition { DeviceType = DeviceType.Hid, VendorId = 0x413d, ProductId = 0x2107 } })).ToList();
            var device = devices[1];
            await device.InitializeAsync();

            var buffer = new byte[9] { 0x00, 0x01, 0x80, 0x33, 0x01, 0x00, 0x00, 0x00, 0x00 };

            var data = await device.WriteAndReadAsync(buffer);
            int temperatureTimesOneHundred = (data.Data[4] & 0xFF) + (data.Data[3] << 8);

            //TODO: Get the humidity

            //Note sometimes the divisor is 256...
            //https://github.com/ccwienk/temper/blob/600755de6b9ccd8d481c4844fa08185acd13aef0/temper.py#L113
            var temperature = Math.Round(temperatureTimesOneHundred / 100.0m, 2, MidpointRounding.ToEven);
        }
        #endregion

    }
}
