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
        private readonly SemaphoreSlim _PollingSemaphoreSlim = new SemaphoreSlim(1, 1);
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
        public DeviceListener(IEnumerable<DeviceDefinition> registeredDevices, int pollMilliseconds)
        {
            DeviceDefinitions.AddRange(registeredDevices);
            _PollTimer = new timer(pollMilliseconds);
            _PollTimer.Elapsed += _PollTimer_Elapsed;
            _PollTimer.Start();
        }
        #endregion

        #region Event Handlers
        private async void _PollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await CheckForDevicesAsync();
        }
        #endregion

        #region Private Methods
        private async Task CheckForDevicesAsync()
        {
            try
            {
                await _PollingSemaphoreSlim.WaitAsync();

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
                        device = DeviceManager.Current.GetDevice(deviceDefinition);
                        _CreatedDevicesByDefinition.Add(deviceDefinition, device);
                    }

                    if (!device.IsInitialized)
                    {
                        device.DeviceId = connectedDeviceDefinition.DeviceId;

                        //The device is not initialized so initialize it
                        await device.InitializeAsync();

                        //Let listeners know a registered device was initialized
                        DeviceInitialized?.Invoke(this, new DeviceEventArgs(device));
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
                        }
                    }
                }

                foreach (var removeDef in removeDefs)
                {
                    _CreatedDevicesByDefinition.Remove(removeDef);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Hid polling error", ex, nameof(DeviceListener));

                //TODO: What else to do here?
            }
            finally
            {
                _PollingSemaphoreSlim.Release();
            }
        }
        #endregion

        #region Public Methods
        public void Stop()
        {
            _PollTimer.Stop();
        }
        #endregion
    }
}
