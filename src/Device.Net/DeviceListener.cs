using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using timer = System.Timers.Timer;

namespace Device.Net
{
    public class DeviceListener
    {
        #region Fields
        private readonly timer _PollTimer;
        private readonly SemaphoreSlim _ListenSemaphoreSlim = new SemaphoreSlim(1, 1);
        private Dictionary<DeviceDefinition, IDevice> _CreatedDevicesByDefinition { get; } = new Dictionary<DeviceDefinition, IDevice>();
        #endregion

        #region Public Properties
        public List<DeviceDefinition> DeviceDefinitions { get; } = new List<DeviceDefinition>();
        #endregion

        #region Events
        public event EventHandler<DeviceEventArgs> DeviceInitialized;
        public event EventHandler<DeviceEventArgs> DeviceDisconnected;
        #endregion

        #region Constructor
        /// <summary>
        /// Handles connecting to and disconnecting from a set of potential devices by their definition
        /// </summary>
        /// <param name="registeredDevices">Device definitions to connect to and disconnect from</param>
        /// <param name="pollMilliseconds">Poll interval in milliseconds, or null if checking is called externally</param>
        public DeviceListener(IEnumerable<DeviceDefinition> registeredDevices, int? pollMilliseconds)
        {
            DeviceDefinitions.AddRange(registeredDevices);
            if (pollMilliseconds.HasValue)
            {
                _PollTimer = new timer(pollMilliseconds.Value);
                _PollTimer.Elapsed += _PollTimer_Elapsed;
                _PollTimer.Start();
            }
        }
        #endregion

        #region Event Handlers
        private async void _PollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await CheckForDevicesAsync();
        }
        #endregion

        #region Public Methods
        public async Task CheckForDevicesAsync()
        {
            try
            {
                await _ListenSemaphoreSlim.WaitAsync();

                var connectedDeviceDefinitions = new List<DeviceDefinition>();
                foreach (var vidPid in DeviceDefinitions)
                {
                    connectedDeviceDefinitions.AddRange(await DeviceManager.Current.GetConnectedDeviceDefinitions(vidPid.VendorId, vidPid.ProductId));
                }

                //Iterate through connected devices
                foreach (var connectedDeviceDefinition in connectedDeviceDefinitions)
                {
                    var deviceDefinition = DeviceDefinitions.FirstOrDefault(d => DeviceManager.IsDefinitionMatch(d, connectedDeviceDefinition));

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

                    if (!device.IsInitialized)
                    {
                        device.DeviceId = connectedDeviceDefinition.DeviceId;

                        //The device is not initialized so initialize it
                        await device.InitializeAsync();

                        //Let listeners know a registered device was initialized
                        DeviceInitialized?.Invoke(this, new DeviceEventArgs(device));

                        Logger.Log("Device connected", null, nameof(DeviceListener));
                    }

                }

                var removeDefs = new List<DeviceDefinition>();

                //Iterate through registered devices
                foreach (var key in _CreatedDevicesByDefinition.Keys)
                {
                    var device = _CreatedDevicesByDefinition[key];

                    if (!connectedDeviceDefinitions.Any(d => DeviceManager.IsDefinitionMatch(d, key)))
                    {
                        if (device.IsInitialized)
                        {
                            //Let listeners know a registered device was disconnected
                            //NOTE: let the rest of the app know before disposal so that the app can stop doing whatever it's doing.
                            DeviceDisconnected?.Invoke(this, new DeviceEventArgs(device));

                            //The device is no longer connected so disconnect it
                            device.Dispose();

                            removeDefs.Add(key);

                            Logger.Log("Disconnected", null, nameof(DeviceListener));
                        }
                    }
                }

                foreach (var removeDef in removeDefs)
                {
                    _CreatedDevicesByDefinition.Remove(removeDef);
                }

                Logger.Log("did a poll", null, nameof(DeviceListener));

            }
            catch (Exception ex)
            {
                Logger.Log("Hid polling error", ex, nameof(DeviceListener));

                //TODO: What else to do here?
            }
            finally
            {
                _ListenSemaphoreSlim.Release();
            }
        }

        public void Stop()
        {
            _PollTimer.Stop();
        }
        #endregion
    }
}
