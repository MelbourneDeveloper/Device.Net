using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net
{
    public class DeviceDataStreamer : IDisposable
    {
        private bool disposed;
        private bool _isRunning;
        private readonly Func<IDevice, Task> _processData;
        private readonly IDeviceFactory _deviceFactory;
        private IDevice _currentDevice;
        private readonly TimeSpan? _interval;
        private readonly ILogger _logger;
        private readonly Func<IDevice, Task> _initializeFunc;

        public DeviceDataStreamer(
            Func<IDevice, Task> processData,
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
            _initializeFunc = initializeFunc ?? (d => d.InitializeAsync());
        }

        public DeviceDataStreamer Start()
        {
            _isRunning = true;

            _ = Task.Run(async () =>
              {
                  while (_isRunning)
                  {
                      await Task.Delay(_interval.Value).ConfigureAwait(false);

                      try
                      {
                          if (_currentDevice == null)
                          {
                              var connectedDevices = await _deviceFactory.GetConnectedDeviceDefinitionsAsync().ConfigureAwait(false);
                              var firstConnectedDevice = connectedDevices.FirstOrDefault();

                              if (firstConnectedDevice == null)
                              {
                                  continue;
                              }

                              _currentDevice = await _deviceFactory.GetDeviceAsync(firstConnectedDevice).ConfigureAwait(false);
                              await _initializeFunc(_currentDevice).ConfigureAwait(false);
                          }

                          await _processData(_currentDevice).ConfigureAwait(false);
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
        public void Dispose()
        {
            if (disposed)
            {
                _logger.LogWarning(Messages.WarningMessageAlreadyDisposed, _currentDevice?.DeviceId);
                return;
            }

            disposed = true;

            _logger.LogInformation("Disposing {deviceId}", _currentDevice?.DeviceId);

            _isRunning = false;
        }
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    }
}
