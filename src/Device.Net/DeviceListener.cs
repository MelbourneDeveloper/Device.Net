using Device.Net.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using timer = System.Timers.Timer;

namespace Device.Net
{
    public sealed class DeviceListener : IDisposable
    {
        #region Fields
        private bool _IsDisposed;
        private readonly timer _PollTimer;
        private readonly SemaphoreSlim _ListenSemaphoreSlim = null;

        /// <summary>
        /// This is the list of Devices by their filter definition. Note this is not actually keyed by the connected definition.
        /// </summary>
        private readonly Dictionary<FilterDeviceDefinition, IDevice> _CreatedDevicesByDefinition = new Dictionary<FilterDeviceDefinition, IDevice>();
        #endregion

        #region Public Properties
        public List<FilterDeviceDefinition> FilterDeviceDefinitions { get; } = new List<FilterDeviceDefinition>();
        public ILogger Logger { get; set; }
        #endregion

        #region Events
        public event EventHandler<DeviceEventArgs> DeviceInitialized;
        public event EventHandler<DeviceEventArgs> DeviceDisconnected;
        #endregion

        #region Constructor
        /// <summary>
        /// Handles connecting to and disconnecting from a set of potential devices by their definition
        /// </summary>
        /// <param name="filterDeviceDefinitions">Device definitions to connect to and disconnect from</param>
        /// <param name="pollMilliseconds">Poll interval in milliseconds, or null if checking is called externally</param>
        public DeviceListener(IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions, int? pollMilliseconds)
        {
            FilterDeviceDefinitions.AddRange(filterDeviceDefinitions);
            _ListenSemaphoreSlim = new SemaphoreSlim(1, 1);
            if (!pollMilliseconds.HasValue) return;

            _PollTimer = new timer(pollMilliseconds.Value);
            _PollTimer.Elapsed += _PollTimer_Elapsed;
        }
        #endregion

        #region Event Handlers
        private async void _PollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_IsDisposed)
                return;
            await CheckForDevicesAsync();
        }
        #endregion

        #region Private Methods
        private void Log(string message, Exception ex, [CallerMemberName] string callerMemberName = null)
        {
            Logger?.Log(message, $"{ nameof(DeviceListener)} - {callerMemberName}", ex, ex != null ? LogLevel.Error : LogLevel.Information);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the polling for devices if polling is being used.
        /// </summary>
        public void Start()
        {
            if (_PollTimer == null)
            {
                throw new ValidationException(Messages.ErrorMessagePollingNotEnabled);
            }

            if (DeviceManager.Current.DeviceFactories.Count == 0) throw new DeviceFactoriesNotRegisteredException();

            _PollTimer.Start();
        }

        public async Task CheckForDevicesAsync()
        {
            try
            {
                if (_IsDisposed) return;
                await _ListenSemaphoreSlim.WaitAsync();

                var connectedDeviceDefinitions = new List<ConnectedDeviceDefinition>();
                foreach (var deviceDefinition in FilterDeviceDefinitions)
                {
                    connectedDeviceDefinitions.AddRange(await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(deviceDefinition));
                }

                //Iterate through connected devices
                foreach (var connectedDeviceDefinition in connectedDeviceDefinitions)
                {
                    var deviceDefinition = FilterDeviceDefinitions.FirstOrDefault(d => DeviceManager.IsDefinitionMatch(d, connectedDeviceDefinition));

                    if (deviceDefinition == null) continue;

                    //TODO: What to do if there are multiple?

                    IDevice device = null;
                    if (_CreatedDevicesByDefinition.ContainsKey(deviceDefinition))
                    {
                        device = _CreatedDevicesByDefinition[deviceDefinition];
                    }

                    if (device == null)
                    {
                        //Need to use the connected device def here instead of the filter version because the filter version won't have the id or any details
                        device = DeviceManager.Current.GetDevice(connectedDeviceDefinition);
                        _CreatedDevicesByDefinition.Add(deviceDefinition, device);
                    }

                    if (device.IsInitialized) continue;

                    Log($"Attempting to initialize with DeviceId of {device.DeviceId}", null);

                    //The device is not initialized so initialize it
                    await device.InitializeAsync();

                    //Let listeners know a registered device was initialized
                    DeviceInitialized?.Invoke(this, new DeviceEventArgs(device));

                    Log(Messages.InformationMessageDeviceConnected, null);
                }

                var removeDefs = new List<FilterDeviceDefinition>();

                //Iterate through registered devices
                foreach (var filteredDeviceDefinitionKey in _CreatedDevicesByDefinition.Keys)
                {
                    var device = _CreatedDevicesByDefinition[filteredDeviceDefinitionKey];

                    if (connectedDeviceDefinitions.Any(cdd =>
                        DeviceManager.IsDefinitionMatch(filteredDeviceDefinitionKey, cdd))) continue;

                    if (!device.IsInitialized) continue;

                    //Let listeners know a registered device was disconnected
                    //NOTE: let the rest of the app know before disposal so that the app can stop doing whatever it's doing.
                    DeviceDisconnected?.Invoke(this, new DeviceEventArgs(device));

                    //The device is no longer connected so close it
                    device.Close();

                    removeDefs.Add(filteredDeviceDefinitionKey);

                    Log(Messages.InformationMessageDeviceListenerDisconnected, null);
                }

                foreach (var removeDef in removeDefs)
                {
                    _CreatedDevicesByDefinition.Remove(removeDef);
                }

                Log(Messages.InformationMessageDeviceListenerPollingComplete, null);

            }
            catch (Exception ex)
            {
                Log(Messages.ErrorMessagePollingError, ex);

                //TODO: What else to do here?
            }
            finally
            {
                if (!_IsDisposed)
                    _ListenSemaphoreSlim.Release();
            }
        }

        public void Stop()
        {
            _PollTimer.Stop();
        }

        public void Dispose()
        {
            if (_IsDisposed) return;
            _IsDisposed = true;

            Stop();

            _PollTimer?.Dispose();

            foreach (var key in _CreatedDevicesByDefinition.Keys)
            {
                _CreatedDevicesByDefinition[key].Dispose();
            }

            _CreatedDevicesByDefinition.Clear();

            _ListenSemaphoreSlim.Dispose();

            DeviceInitialized = null;
            DeviceDisconnected = null;

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Finalizer
        ~DeviceListener()
        {
            Dispose();
        }
        #endregion
    }
}
