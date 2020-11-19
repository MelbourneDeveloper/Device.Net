using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net
{

    public delegate Task ProcessData(IDevice device);

    public class DeviceDataStreamer : IDisposable
    {
        private bool _isRunning;
        private readonly ProcessData _processData;
        private readonly IDeviceFactory _deviceFactory;
        private IDevice _currentDevice;
        private readonly TimeSpan? _interval;
        private readonly ILogger _logger;
        private readonly Func<IDevice, Task> _initializeFunc;

        public DeviceDataStreamer(
            ProcessData processData,
            IDeviceFactory deviceFactory,
            TimeSpan? interval = null,
            ILoggerFactory loggerFactory = null,
            Func<IDevice, Task> initializeFunc = null
          )
        {
            _processData = processData;
            _deviceFactory = deviceFactory;
            _interval = interval ?? new TimeSpan(0, 0, 1);
            _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<DeviceDataStreamer>();
            _initializeFunc = initializeFunc ?? new Func<IDevice, Task>((d) => d.InitializeAsync());
        }

        public DeviceDataStreamer Start()
        {
            _isRunning = true;

            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    await Task.Delay(_interval.Value);

                    try
                    {
                        if (_currentDevice == null)
                        {
                            var connectedDevices = await _deviceFactory.GetConnectedDeviceDefinitionsAsync();
                            var firstConnectedDevice = connectedDevices.FirstOrDefault();

                            if (firstConnectedDevice == null)
                            {
                                continue;
                            }

                            _currentDevice = await _deviceFactory.GetDevice(firstConnectedDevice);
                            await _initializeFunc(_currentDevice);
                        }

                        await _processData(_currentDevice);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing");
                        _currentDevice?.Dispose();
                        _currentDevice = null;
                    }
                }
            });

            return this;
        }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose() => _isRunning = false;
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    }
}
