using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net.Reactive
{
    public class ReactiveDeviceManager : IDisposable, IReactiveDeviceManager
    {
        #region Fields
        private readonly ILogger<ReactiveDeviceManager> _logger;
        private readonly Func<IDevice, Task> _initializeDeviceAction;
        private readonly IObserver<ConnectedDevice> _initializedDeviceObserver;
        private readonly ObserverFactory<IReadOnlyCollection<ConnectedDevice>> _connectedDevicesObserverFactory;
        private IDevice _selectedDevice;
        private readonly IDisposable _selectedDeviceObserver;
        private readonly int _pollMilliseconds;
        #endregion

        #region Protected Properties
        protected IDeviceManager DeviceManager { get; }
        #endregion

        #region Public Properties
        public IList<FilterDeviceDefinition> FilterDeviceDefinitions { get; }
        public Func<IObserver<IReadOnlyCollection<ConnectedDevice>>, IDisposable> SubscribeToConnectedDevices => _connectedDevicesObserverFactory.Subscribe;

        public IDevice SelectedDevice
        {
            get => _selectedDevice;
            private set
            {
                _selectedDevice = value;
                _initializedDeviceObserver.OnNext(value != null ? new ConnectedDevice { DeviceId = value.DeviceId } : null);
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceManager">/param>
        /// <param name="selectedDeviceObservable">Listens for a selected device</param>
        /// <param name="initializedDeviceObserver">Tells others that the device was connected</param>
        /// <param name="connectedDevicesObserver">Tells others which devices are connected</param>
        /// <param name="loggerFactory"></param>
        public ReactiveDeviceManager(
            IDeviceManager deviceManager,
            IObservable<ConnectedDevice> selectedDeviceObservable,
            IObserver<ConnectedDevice> initializedDeviceObserver,
            ILoggerFactory loggerFactory,
            Func<IDevice, Task> initializeDeviceAction,
            IList<FilterDeviceDefinition> filterDeviceDefinitions,
            int pollMilliseconds
            )
        {
            DeviceManager = deviceManager;
            _selectedDeviceObserver = selectedDeviceObservable.Subscribe((d) => InitializeDeviceAsync(d));
            _logger = loggerFactory.CreateLogger<ReactiveDeviceManager>();
            _initializedDeviceObserver = initializedDeviceObserver;
            _connectedDevicesObserverFactory = new ObserverFactory<IReadOnlyCollection<ConnectedDevice>>(
            GetConnectedDevicesAsync
            );

            FilterDeviceDefinitions = filterDeviceDefinitions;

            _initializeDeviceAction = initializeDeviceAction;
            _pollMilliseconds = pollMilliseconds;
        }
        #endregion

        #region Public Methods
        public async Task<TResponse> WriteAndReadAsync<TRequest, TResponse>(TRequest request, Func<byte[], TResponse> convertFunc) where TRequest : IRequest
        {
            try
            {
                var writeBuffer = request.ToArray();
                var readBuffer = await SelectedDevice.WriteAndReadAsync(writeBuffer);
                return convertFunc(readBuffer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                _initializedDeviceObserver.OnError(ex);

                if (ex is IOException)
                {
                    //The exception was an IO exception so disconnect the device
                    //The listener should reconnect
                    SelectedDevice = null;
                }

                throw;
            }
        }

        public void Dispose() => _selectedDeviceObserver.Dispose();
        #endregion

        #region Private Methods
        private async Task<IReadOnlyCollection<ConnectedDevice>> GetConnectedDevicesAsync()
        {
            var devices = await DeviceManager.GetDevicesAsync(FilterDeviceDefinitions);

            var lists = devices.Select(d => new ConnectedDevice { DeviceId = d.DeviceId }).ToList();

            //TODO: This should be moved. This will cause a 1 second wait the first time around.
            await Task.Delay(_pollMilliseconds);

            return new ReadOnlyCollection<ConnectedDevice>(lists);
        }

        private async Task InitializeDeviceAsync(ConnectedDevice connectedDevice)
        {
            try
            {
                var device = DeviceManager.GetDevice(new ConnectedDeviceDefinition(connectedDevice.DeviceId));
                await _initializeDeviceAction(device);

                _logger.LogInformation("Device initialized {deviceId}", connectedDevice.DeviceId);
                SelectedDevice = device;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                _initializedDeviceObserver.OnError(ex);
                SelectedDevice = null;
            }
        }
        #endregion
    }
}
