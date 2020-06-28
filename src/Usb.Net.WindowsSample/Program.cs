using System.Threading;
using System.Threading.Tasks;
using Device.Net;
using System.Collections.Generic;

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
            var devices = await _DeviceManager.GetDevicesAsync(new List<FilterDeviceDefinition> { new FilterDeviceDefinition { DeviceType = DeviceType.Hid, VendorId = 0x0413, ProductId = 0x2107 } });
        }
        #endregion

    }
}
