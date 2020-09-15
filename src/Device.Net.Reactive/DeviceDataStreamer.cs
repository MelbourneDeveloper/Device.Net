using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net.Reactive
{

    public delegate Task ProcessData<T>(IDevice device);

    public class DeviceDataStreamer<T> : IDisposable
    {
        private bool _isRunning;
        private readonly ProcessData<T> _processData;
        private readonly IDeviceManager _deviceManager;
        private IDevice _currentDevice;
        private readonly TimeSpan? _interval;
        private readonly ILogger _logger;

        public DeviceDataStreamer(
            ProcessData<T> processData,
            IDeviceManager deviceManager,
            TimeSpan? interval = null,
            ILoggerFactory loggerFactory = null
            )
        {
            _processData = processData;
            _deviceManager = deviceManager;
            _interval = interval ?? new TimeSpan(0, 0, 1);
            _logger = (loggerFactory ?? new DummyLoggerFactory()).CreateLogger<DeviceDataStreamer<T>>();
        }

        public void Start()
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
                            var connectedDevices = await _deviceManager.GetConnectedDeviceDefinitionsAsync();
                            var firstConnectedDevice = connectedDevices.FirstOrDefault();

                            if (firstConnectedDevice == null)
                            {
                                continue;
                            }

                            _currentDevice = await _deviceManager.GetDevice(firstConnectedDevice);
                            await _currentDevice.InitializeAsync();
                        }

                        await _processData(_currentDevice);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing");
                    }
                }
            });
        }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose() => _isRunning = false;
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    }
}
