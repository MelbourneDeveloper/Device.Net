using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net
{
    /// <summary>
    /// This class is a work in progress. It is not production ready. Documentation: href="https://melbournedeveloper.github.io/Device.Net/articles/DeviceManager.html"
    /// </summary>
    public class DeviceManager : IDeviceManager, IDisposable
    {
        #region Fields
        private readonly ILogger<DeviceManager> _logger;
        private readonly Func<IDevice, Task> _initializeDeviceAction;
        private IDevice _selectedDevice;
        private readonly Queue<IRequest> _queuedRequests = new();
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private readonly SemaphoreSlim _semaphoreSlim2 = new(1, 1);
        private readonly DeviceNotify _notifyDeviceInitialized;
        private readonly NotifyDeviceException _notifyDeviceException;
        private readonly DevicesNotify _notifyConnectedDevices;
        private bool isDisposed;
        private readonly int _pollMilliseconds;
        private readonly GetConnectedDevicesAsync _getConnectedDevicesAsync;
        private readonly GetDeviceAsync _getDevice;
        #endregion

        #region Public Properties
        /// <summary>
        /// Placeholder. Don't use. This functionality will be injected in
        /// </summary>
        public bool FilterMiddleMessages { get; set; }

        public IObservable<IReadOnlyCollection<ConnectedDeviceDefinition>> ConnectedDevicesObservable { get; }

        public IDevice SelectedDevice
        {
            get => _selectedDevice;
            private set
            {
                _selectedDevice = value;
                _notifyDeviceInitialized(value?.ConnectedDeviceDefinition);
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="notifyDeviceInitialized">Tells others that the device was initialized</param>
        /// <param name="notifyConnectedDevices">Tells others which devices are connected</param>
        /// <param name="notifyDeviceException"></param>
        /// <param name="initializeDeviceAction"></param>
        /// <param name="getConnectedDevicesAsync"></param>
        /// <param name="getDevice"></param>
        /// <param name="pollMilliseconds"></param>
        /// <param name="loggerFactory"></param>
        public DeviceManager(
            DeviceNotify notifyDeviceInitialized,
            DevicesNotify notifyConnectedDevices,
            NotifyDeviceException notifyDeviceException,
            Func<IDevice, Task> initializeDeviceAction,
            GetConnectedDevicesAsync getConnectedDevicesAsync,
            GetDeviceAsync getDevice,
            int pollMilliseconds,
            ILoggerFactory loggerFactory = null)
        {
            _notifyDeviceInitialized = notifyDeviceInitialized ?? throw new ArgumentNullException(nameof(notifyDeviceInitialized));
            _notifyDeviceException = notifyDeviceException ?? throw new ArgumentNullException(nameof(notifyDeviceException));
            _notifyConnectedDevices = notifyConnectedDevices ?? throw new ArgumentNullException(nameof(notifyConnectedDevices));
            _getConnectedDevicesAsync = getConnectedDevicesAsync ?? throw new ArgumentNullException(nameof(getConnectedDevicesAsync));
            _getDevice = getDevice ?? throw new ArgumentNullException(nameof(getDevice));

            _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<DeviceManager>();

            _initializeDeviceAction = initializeDeviceAction;
            _pollMilliseconds = pollMilliseconds;
        }
        #endregion

        #region Public Methods
        public void Start()
        {
            _ = Task.Run(async () =>
            {
                while (!isDisposed)
                {
                    var devices = await _getConnectedDevicesAsync();
                    _logger.LogInformation("Found {deviceCount} devices", devices.Count);
                    _notifyConnectedDevices(devices);
                    await Task.Delay(TimeSpan.FromMilliseconds(_pollMilliseconds));
                }
            });
        }

        /// <summary>
        /// Sets the selected device
        /// </summary>
        /// <param name="connectedDevice"></param>
        public void SelectDevice(ConnectedDeviceDefinition connectedDevice)
        {
            _ = connectedDevice == null
                ? throw new ArgumentNullException(nameof(connectedDevice))
                : InitializeDeviceAsync(connectedDevice);
        }

        public async Task<TResponse> WriteAndReadAsync<TResponse>(IRequest request, Func<byte[], TResponse> convertFunc)
        {
            if (SelectedDevice == null) return default;
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                await _semaphoreSlim.WaitAsync();
                var writeBuffer = request.ToArray();
                var readBuffer = await SelectedDevice.WriteAndReadAsync(writeBuffer);
                return convertFunc != null ? convertFunc(readBuffer) : default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                if (ex is not IOException) throw;

                _notifyDeviceException(SelectedDevice?.ConnectedDeviceDefinition, ex);
                //The exception was an IO exception so disconnect the device
                //The listener should reconnect

                SelectedDevice.Dispose();

                SelectedDevice = null;

                throw;
            }
            finally
            {
                _ = _semaphoreSlim.Release();
            }
        }

        public void QueueRequest(IRequest request)
        {
            //If ther is no device selected just eat up the messages
            if (SelectedDevice == null) return;

            if (request == null) throw new ArgumentNullException(nameof(request));

            _queuedRequests.Enqueue(request);
            _ = ProcessQueue();
        }

        private async Task ProcessQueue()
        {
            try
            {
                await _semaphoreSlim2.WaitAsync();

                IRequest mostRecentRequest = null;

                if (_queuedRequests.Count == 0) return;

                if (FilterMiddleMessages)
                {
                    //Eat requests except for the most recent one
                    while (_queuedRequests.Count > 0)
                    {
                        mostRecentRequest = _queuedRequests.Dequeue();
                    }
                }

                _ = await WriteAndReadAsync<object>(mostRecentRequest, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                _ = _semaphoreSlim2.Release();
            }
        }
        #endregion

        #region Private Methods
        private async Task InitializeDeviceAsync(ConnectedDeviceDefinition connectedDevice)
        {
            try
            {
                if (connectedDevice == null)
                {
                    _logger.LogInformation("Initialize requested but device was null");
                    SelectedDevice = null;
                    return;
                }

                var device = await _getDevice(connectedDevice);
                await _initializeDeviceAction(device);

                _logger.LogInformation("Device initialized {deviceId}", connectedDevice.DeviceId);
                SelectedDevice = device;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                _notifyDeviceException(connectedDevice, ex);
                SelectedDevice = null;
            }
        }

        public void Dispose()
        {
            isDisposed = true;
            _semaphoreSlim.Dispose();
            _semaphoreSlim2.Dispose();
        }
        #endregion
    }
}
