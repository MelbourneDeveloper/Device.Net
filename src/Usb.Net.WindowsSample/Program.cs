using Hid.Net.Windows;
using System;
using System.Threading;
using System.Threading.Tasks;
using Usb.Net.Sample;
using Usb.Net.Windows;

namespace Usb.Net.WindowsSample
{
    internal class Program
    {
        #region Fields
        private static TrezorExample _DeviceConnectionExample = new TrezorExample();
        #endregion

        #region Main
        private static void Main(string[] args)
        {
            //Register the factory for creating Usb devices. This only needs to be done once.
            WindowsUsbDeviceFactory.Register();
            WindowsHidDeviceFactory.Register();

            _DeviceConnectionExample.TrezorInitialized += _DeviceConnectionExample_TrezorInitialized;
            _DeviceConnectionExample.TrezorDisconnected += _DeviceConnectionExample_TrezorDisconnected;

            var wait = new ManualResetEvent(false);

            Go(Menu());

            wait.WaitOne();
        }

        private static async Task Go(int menuOption)
        {
            switch (menuOption)
            {
                case 1:
                    await _DeviceConnectionExample.InitializeTrezorAsync();
                    var bytes = await _DeviceConnectionExample.WriteAndReadFromDeviceAsync();
                    break;
                case 2:
                    _DeviceConnectionExample.StartListenting();
                    break;
            }
        }
        #endregion

        #region Event Handlers
        private static void _DeviceConnectionExample_TrezorDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected");
        }

        private static void _DeviceConnectionExample_TrezorInitialized(object sender, EventArgs e)
        {
            Console.WriteLine("Initialized");
        }
        #endregion

        #region Private Methods
        private static int Menu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. Write To Connected Device");
                Console.WriteLine("2. Listen For Device");
                var consoleKey = Console.ReadKey();
                if (consoleKey.KeyChar == '1') return 1;
                if (consoleKey.KeyChar == '2') return 2;
            }
        }

        private void DisplayData(byte[] readBuffer)
        {
            Console.WriteLine(string.Join(' ', readBuffer));
            Console.ReadKey();
        }
        #endregion
    }
}
