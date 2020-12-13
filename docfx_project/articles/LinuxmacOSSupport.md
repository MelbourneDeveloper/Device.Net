**Linux and MacOS support is here! Check out the [MacOS sample](https://github.com/MelbourneDeveloper/Device.Net/tree/develop/src/Device.Net.MacOSLibUsbSample) and the [Linux/MacOS/Windows Terminal/Console App](https://github.com/MelbourneDeveloper/Device.Net/tree/develop/src/Device.Net.LibUsbSample). Grab the NuGet for [Device.Net.LibUsb](https://www.nuget.org/packages/Device.Net.LibUsb/).**

The most common Linux/Windows cross platform library is called [LibUsb](https://github.com/libusb/libusb). There is a C# wrapper for it called [LibUsbDotNet](https://github.com/LibUsbDotNet/LibUsbDotNet). This library wrapper is leveraged in [Device.Net.LibUsb](https://www.nuget.org/packages/Device.Net.LibUsb/) to bring Linux and MacOS support to your Device.Net based app. As long as you use the [IDevice interface](https://github.com/MelbourneDeveloper/Device.Net/wiki/IDevice) across platforms, code will be 100% compatible. The IDevice implementation is below. 

**Please [install LibUsb](https://libusb.info/)! If it is not installed on your machine, the samples will not work!**

In the long term, I'm still looking to build a Linux, and MacOS library from the ground up without the LibUsb dependency, so please get it contact if you'd like to contribute. But, For now, Device.Net.LibUsb will get you over the hump.

```cs
    public class LibUsbDevice : IDevice
    {
        #region Fields
        private UsbEndpointReader _UsbEndpointReader;
        private UsbEndpointWriter _UsbEndpointWriter;
        private int ReadPacketSize;
        private SemaphoreSlim _WriteAndReadLock = new SemaphoreSlim(1, 1);
        #endregion

        #region Public Properties
        public UsbDevice UsbDevice { get; }
        public int VendorId => GetVendorId(UsbDevice);
        public int ProductId => GetProductId(UsbDevice);
        public int Timeout { get; }
        public bool IsInitialized { get; private set; }
        public ConnectedDeviceDefinitionBase ConnectedDeviceDefinition => throw new NotImplementedException();
        public string DeviceId => UsbDevice.DevicePath;
        #endregion

        #region Events
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        #endregion

        #region Constructor
        public LibUsbDevice(UsbDevice usbDevice, int timeout)
        {
            UsbDevice = usbDevice;
            Timeout = timeout;
        }
        #endregion

        #region Implementation
        public void Dispose()
        {
            //TODO: Release the device...
            // UsbDevice.Dispose();
        }


        public async Task InitializeAsync()
        {
            await Task.Run(() =>
            {

                //TODO: Error handling etc.
                UsbDevice.Open();

                //TODO: This is far beyond not cool.
                if (UsbDevice is MonoUsbDevice monoUsbDevice)
                {
                    monoUsbDevice.ClaimInterface(0);
                }
                else if (UsbDevice is WinUsbDevice winUsbDevice)
                {
                    //Doesn't seem necessary in this case...
                }
                else
                {
                    ((IUsbDevice)UsbDevice).ClaimInterface(0);
                }

                _UsbEndpointWriter = UsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
                _UsbEndpointReader = UsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                ReadPacketSize = _UsbEndpointReader.EndpointInfo.Descriptor.MaxPacketSize;

                IsInitialized = true;
            });
        }

        public async Task<byte[]> ReadAsync()
        {
            await _WriteAndReadLock.WaitAsync();

            try
            {
                return await Task.Run(() =>
                {
                    var buffer = new byte[ReadPacketSize];

                    _UsbEndpointReader.Read(buffer, Timeout, out var bytesRead);

                    return buffer;
                });
            }
            finally
            {
                _WriteAndReadLock.Release();
            }
        }

        public async Task WriteAsync(byte[] data)
        {
            await _WriteAndReadLock.WaitAsync();

            try
            {
                await Task.Run(() =>
                {
                    _UsbEndpointWriter.Write(data, Timeout, out var bytesWritten);
                });
            }
            finally
            {
                _WriteAndReadLock.Release();
            }
        }

        public async Task<byte[]> WriteAndReadAsync(byte[] writeBuffer)
        {
            await WriteAsync(writeBuffer);
            return await ReadAsync();
        }
        #endregion

        #region Public Static Methods
        public static int GetVendorId(UsbDevice usbDevice)
        {
            if (usbDevice is MonoUsbDevice monoUsbDevice)
            {
                return monoUsbDevice.Profile.DeviceDescriptor.VendorID;
            }
            else
            {
                return usbDevice.UsbRegistryInfo.Vid;
            }
        }

        public static int GetProductId(UsbDevice usbDevice)
        {
            if (usbDevice is MonoUsbDevice monoUsbDevice)
            {
                return monoUsbDevice.Profile.DeviceDescriptor.ProductID;
            }
            else
            {
                return usbDevice.UsbRegistryInfo.Pid;
            }
        }
        #endregion
    }
```
[Code Reference](https://github.com/MelbourneDeveloper/Device.Net/blob/e195ee2c993608f81aba67fbe48db1f5d0954905/src/Device.Net.LibUsb/LibUsbDevice.cs#L10)

Here is a cross platform Terminal/Console sample (Linux, MacOS, Windows):

```cs
    internal class Program
    {
        #region Fields
        private static TrezorExample _DeviceConnectionExample = new TrezorExample();
        #endregion

        #region Main
        private static void Main(string[] args)
        {
            //Register the factory for creating Usb devices. This only needs to be done once.
#if (LIBUSB)
            LibUsbUsbDeviceFactory.Register();
#else
            WindowsUsbDeviceFactory.Register();
            WindowsHidDeviceFactory.Register();
#endif

            _DeviceConnectionExample.TrezorInitialized += _DeviceConnectionExample_TrezorInitialized;
            _DeviceConnectionExample.TrezorDisconnected += _DeviceConnectionExample_TrezorDisconnected;

            Go(Menu());

            new ManualResetEvent(false).WaitOne();
        }

        private static async Task Go(int menuOption)
        {
            switch (menuOption)
            {
                case 1:
                    try
                    {
                        await _DeviceConnectionExample.InitializeTrezorAsync();
                        await DisplayDataAsync();
                        _DeviceConnectionExample.Dispose();
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
            }
        }
        #endregion

        #region Event Handlers
        private static void _DeviceConnectionExample_TrezorDisconnected(object sender, EventArgs e)
        {
            Console.Clear();
            Console.WriteLine("Disconnnected.");
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
        private static int Menu()
        {
            while (true)
            {
                Console.Clear();
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
```

[Trezor Example Code Reference](https://github.com/MelbourneDeveloper/Device.Net/blob/e195ee2c993608f81aba67fbe48db1f5d0954905/src/Usb.Net.UWP.Sample/TrezorExample.cs#L9)

Here is a simple Mac sample.

```cs
    public partial class ViewController : NSViewController
    {
        TrezorExample TrezorExample = new TrezorExample();

        public ViewController(IntPtr handle) : base(handle)
        {

        }

        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();

            LibUsbUsbDeviceFactory.Register();

            await TrezorExample.InitializeTrezorAsync();
            var buffer = await TrezorExample.WriteAndReadFromDeviceAsync();

            var alert = new NSAlert
            {
                MessageText = $"Got it! {buffer[0]},{buffer[1]},{buffer[2]}.  Warning: Make sure you unplug and replug before trying again.",
                AlertStyle = NSAlertStyle.Informational
            };

            alert.AddButton("OK");

            var returnValue = alert.RunModal();
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }
    }
```
[Code Reference](https://github.com/MelbourneDeveloper/Device.Net/blob/e195ee2c993608f81aba67fbe48db1f5d0954905/src/Device.Net.MacOSLibUsbSample/ViewController.cs#L10)

[HIDSharp](https://github.com/treehopper-electronics/HIDSharp) is another cross platform library that looks promising. It seems to have providers for MacOS and Linux. I haven't tested this, but I'd really like to hear from anyone who has tried it.