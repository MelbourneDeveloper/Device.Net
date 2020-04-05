using System;
using System.Threading;
using System.Threading.Tasks;
using Usb.Net.Sample;
using Device.Net;
using Hid.Net.Windows;

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
        private static TrezorExample _DeviceConnectionExample;
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
            _DeviceManager.RegisterDeviceFactory(new WindowsUsbDeviceFactory(Logger, Tracer));
            _DeviceManager.RegisterDeviceFactory(new WindowsHidDeviceFactory(Logger, Tracer));
            _DeviceManager.RegisterDeviceFactory(new WindowsSerialPortDeviceFactory(Logger, Tracer));
#endif

            _DeviceConnectionExample = new TrezorExample(_DeviceManager);
            _DeviceConnectionExample.TrezorInitialized += _DeviceConnectionExample_TrezorInitialized;
            _DeviceConnectionExample.TrezorDisconnected += _DeviceConnectionExample_TrezorDisconnected;

            Go();

            new ManualResetEvent(false).WaitOne();
        }

        private static async Task Go()
        {
            var asdasd = new WindowsHidDevice(@"\\?\hid#vid_413c&pid_3016#6&876c144&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}");
            await asdasd.InitializeAsync();
            var gasdasd =await asdasd.ReadAsync();
        }
        #endregion

        #region Event Handlers
        private static void _DeviceConnectionExample_TrezorDisconnected(object sender, EventArgs e)
        {
            Console.Clear();
            Console.WriteLine("Disconnected.");
            DisplayWaitMessage();
        }

        private static async void _DeviceConnectionExample_TrezorInitialized(object sender, EventArgs e)
        {
            try
            {
                Console.Clear();
                await DisplayDataAsync();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion

        #region Private Methods
        private async static Task<int> Menu()
        {
            while (true)
            {
                Console.Clear();

                var devices = await _DeviceManager.GetConnectedDeviceDefinitionsAsync(null);
                Console.WriteLine("Currently connected devices: ");
                foreach (var device in devices)
                {
                    Console.WriteLine(device.DeviceId);
                }
                Console.WriteLine();

                Console.WriteLine("Console sample. This sample demonstrates either writing to the first found connected device, or listening for a device and then writing to it. If you listen for the device, you will be able to connect and disconnect multiple times. This represents how users may actually use the device.");
                Console.WriteLine();
                Console.WriteLine("1. Write To Connected Device");
                Console.WriteLine("2. Listen For Device");
                var consoleKey = Console.ReadKey();
                if (consoleKey.KeyChar == '1') return 1;
                if (consoleKey.KeyChar == '2') return 2;
            }
        }

        private static async Task DisplayDataAsync()
        {
            var bytes = await _DeviceConnectionExample.WriteAndReadFromDeviceAsync();
            Console.Clear();
            Console.WriteLine("Device connected. Output:");
            DisplayData(bytes);
        }

        private static void DisplayData(byte[] readBuffer)
        {
            Console.WriteLine(string.Join(' ', readBuffer));
            Console.ReadKey();
        }

        private static void DisplayWaitMessage()
        {
            Console.WriteLine("Waiting for device to be plugged in...");
        }
        #endregion
    }
}
