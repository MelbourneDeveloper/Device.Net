using Device.Net;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Usb.Net.Sample
{
    class Program
    {
        static void Main(string[] args)
        {

            Go();

            Console.ReadLine();
        }

        private static async Task Go()
        {
            var devices = WindowsDeviceBase.GetConnectedDeviceInformations(WindowsDeviceConstants.GUID_DEVINTERFACE_USB_DEVICE);

            //var device = devices.FirstOrDefault(d => d.DevicePath.ToLower().Contains("2b24"));
            var device = devices.FirstOrDefault(d => d.DevicePath.ToLower().Contains("1209"));

            var windowsUsbDevice = new WindowsUsbDevice(device.DevicePath, 64, 64);

            await windowsUsbDevice.InitializeAsync();

            var buffer = new byte[64];
            buffer[0] = 0x3f;
            buffer[1] = 0x23;
            buffer[2] = 0x23;

            await windowsUsbDevice.WriteAsync(buffer);

            var asdasd = await windowsUsbDevice.ReadAsync();
        }
    }
}
