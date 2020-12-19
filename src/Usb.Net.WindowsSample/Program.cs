using System;
using System.Threading.Tasks;
using Usb.Net.Sample;
using Device.Net;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;

#if !LIBUSB
using SerialPort.Net.Windows;
using Hid.Net.Windows;
using Usb.Net.Windows;
using Device.Net.Windows;
#else
using Device.Net.LibUsb;
#endif

namespace Usb.Net.WindowsSample
{
    internal class Program
    {
        #region Fields
        private static ILoggerFactory _loggerFactory;
        private static IDeviceFactory _trezorFactories;

#if !LIBUSB
        private static readonly IDeviceFactory _allFactories = new WindowsSerialPortDeviceFactory(_loggerFactory)
                .Aggregate(WindowsUsbDeviceFactoryExtensions.CreateWindowsUsbDeviceFactory(_loggerFactory))
                .Aggregate(WindowsUsbDeviceFactoryExtensions.CreateWindowsUsbDeviceFactory(_loggerFactory, classGuid: WindowsDeviceConstants.GUID_DEVINTERFACE_USB_DEVICE))
                .Aggregate(WindowsHidDeviceFactoryExtensions.CreateWindowsHidDeviceFactory(_loggerFactory));
#endif

        private static TrezorExample _DeviceConnectionExample;
        #endregion

        #region Main
        private static async Task Main()
        {
            _loggerFactory = LoggerFactory.Create((builder) =>
            {
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            //Register the factories for creating Usb devices. This only needs to be done once.
#if LIBUSB
            _trezorFactories = new List<IDeviceFactory>
            {
                TrezorExample.UsbDeviceDefinitions.CreateLibUsbDeviceFactory(_loggerFactory)
            }.Aggregate(_loggerFactory);
#else
            _trezorFactories = new List<IDeviceFactory>
            {
                TrezorExample.UsbDeviceDefinitions.CreateWindowsUsbDeviceFactory(_loggerFactory),
                TrezorExample.HidDeviceDefinitions.CreateWindowsHidDeviceFactory(_loggerFactory),
            }.Aggregate(_loggerFactory);

#endif

            _DeviceConnectionExample = new TrezorExample(_trezorFactories, _loggerFactory);
            _DeviceConnectionExample.TrezorInitialized += DeviceConnectionExample_TrezorInitialized;
            _DeviceConnectionExample.TrezorDisconnected += DeviceConnectionExample_TrezorDisconnected;

            await GoAsync();
        }

        private static async Task GoAsync()
        {
            var menuOption = await MenuAsync();

            switch (menuOption)
            {
                case 1:
                    try
                    {
                        await _DeviceConnectionExample.InitializeTrezorAsync();
                        await DisplayDataAsync();
                        _DeviceConnectionExample.Dispose();

                        GC.Collect();

                        await Task.Delay(10000);
                    }
                    catch (Exception ex)
                    {
                        Console.Clear();
                        Console.WriteLine(ex.ToString());
                    }
                    Console.ReadKey();
                    break;
                case 2:
                    Console.Clear();
                    DisplayWaitMessage();
                    _DeviceConnectionExample.StartListening();
                    break;
#if !LIBUSB
                case 3:

                    Console.Clear();
                    await DisplayTemperature();
                    while (true)
                    {
                        await Task.Delay(1000);
                    }
#endif
                default:
                    Console.WriteLine("That's not an option");
                    break;
            }
        }

#if !LIBUSB
#pragma warning disable CA2000

        private static async Task DisplayTemperature()
        {
            //Connect to the device by product id and vendor id
            var temperDevice = await new FilterDeviceDefinition(vendorId: 0x413d, productId: 0x2107, usagePage: 65280)
                .CreateWindowsHidDeviceManager(_loggerFactory)
                .ConnectFirstAsync()
                .ConfigureAwait(false);

            //Create the observable
            var observable = Observable
                .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(.1))
                .SelectMany(_ => Observable.FromAsync(() => temperDevice.WriteAndReadAsync(new byte[] { 0x00, 0x01, 0x80, 0x33, 0x01, 0x00, 0x00, 0x00, 0x00 })))
                .Select(data => (data.Data[4] & 0xFF) + (data.Data[3] << 8))
                //Only display the temperature when it changes
                .Distinct()
                .Select(temperatureTimesOneHundred => Math.Round(temperatureTimesOneHundred / 100.0m, 2, MidpointRounding.ToEven));

            //Subscribe to the observable
            observable.Subscribe(t => Console.WriteLine($"Temperature is {t}"));

            //Note: in a real scenario, we would dispose of the subscription afterwards. This method runs forever.
        }

#pragma warning restore CA2000
#endif

        #endregion

        #region Event Handlers
        private static void DeviceConnectionExample_TrezorDisconnected(object sender, EventArgs e)
        {
            Console.Clear();
            Console.WriteLine("Disconnected.");
            DisplayWaitMessage();
        }

        private static async void DeviceConnectionExample_TrezorInitialized(object sender, EventArgs e)
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
#pragma warning disable CS1998 
        private static async Task<int> MenuAsync()
#pragma warning restore CS1998 
        {
            while (true)
            {
                Console.Clear();

#if !LIBUSB
                var devices = await _allFactories.GetConnectedDeviceDefinitionsAsync();

                Console.WriteLine("Currently connected devices:\r\n");
                Console.WriteLine(string.Join("\r\n",
                    devices
                    .OrderBy(d => d.Manufacturer)
                    .ThenBy(d => d.ProductName)
                    .Select(d => $"{d.Manufacturer} - {d.ProductName} ({d.DeviceType} - {d.ClassGuid})\r\nDevice Path: {d.DeviceId}\r\nVendor: {d.VendorId} Product Id: {d.ProductId}\r\n")));
#endif

                Console.WriteLine("Console sample. This sample demonstrates either writing to the first found connected device, or listening for a device and then writing to it. If you listen for the device, you will be able to connect and disconnect multiple times. This represents how users may actually use the device.");
                Console.WriteLine();
                Console.WriteLine("1. Write To Connected Device");
                Console.WriteLine("2. Listen For Device");
                Console.WriteLine("3. Temperature Monitor (Observer Design Pattern - https://docs.microsoft.com/en-us/dotnet/standard/events/how-to-implement-a-provider#example)");
                var consoleKey = Console.ReadKey();
                if (consoleKey.KeyChar == '1') return 1;
                if (consoleKey.KeyChar == '2') return 2;
                if (consoleKey.KeyChar == '3') return 3;
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

        private static void DisplayWaitMessage() => Console.WriteLine("Waiting for device to be plugged in...");
        #endregion

    }
}
