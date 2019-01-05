using Device.Net;
using Hid.Net.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Usb.Net.Windows;

namespace Usb.Net.WindowsSample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //Register the factory for creating Usb devices. This only needs to be done once.
            WindowsUsbDeviceFactory.Register();
            WindowsHidDeviceFactory.Register();

            //Go();
            Poll();
            Console.ReadLine();
        }

        private static async Task Poll()
        {
            var windowsDevice = new WindowsUsbDevice(0x1209, 0x53c1);
            var devicePoller = new DevicePoller(new List<IDevice> { windowsDevice } , 3000);
            devicePoller.DeviceInitialized += DevicePoller_DeviceInitialized;
            devicePoller.DeviceDisconnected += DevicePoller_DeviceDisconnected;
            devicePoller.RegisterDevice(windowsDevice);

            while (true)
            {
                await Task.Delay(1000);
            }
        }

        private static void DevicePoller_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            Console.WriteLine("Disconnected");
        }

        private static void DevicePoller_DeviceInitialized(object sender, DeviceEventArgs e)
        {
            Console.WriteLine("Initialized");
        }

        private static async Task Go()
        {
            try
            {
                //Note: other custom device types could be added here

                //Define the types of devices to search for. This particular device can be connected to via USB, or Hid
                var deviceDefinitions = new List<DeviceDefinition>
                {
                    new DeviceDefinition{ DeviceType= DeviceType.Hid, VendorId= 0x534C, ProductId=0x0001, Label="Trezor One Firmware 1.6.x" },
                    new DeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x1209, ProductId=0x53C1, ReadBufferSize=64, WriteBufferSize=64, Label="Trezor One Firmware 1.7.x" },
                    new DeviceDefinition{ DeviceType= DeviceType.Usb, VendorId= 0x1209, ProductId=0x53C0, ReadBufferSize=64, WriteBufferSize=64, Label="Model T" }
                };

                //Get the first available device and connect to it
                var devices = await DeviceManager.Current.GetDevices(deviceDefinitions);
                using (var trezorDevice = devices.FirstOrDefault())
                {
                    await trezorDevice.InitializeAsync();

                    //Create a buffer with 3 bytes (initialize)
                    var buffer = new byte[64];
                    buffer[0] = 0x3f;
                    buffer[1] = 0x23;
                    buffer[2] = 0x23;

                    //Write the data to the device and get the response
                    var readBuffer = await trezorDevice.WriteAndReadAsync(buffer);

                    Console.WriteLine("All good");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
