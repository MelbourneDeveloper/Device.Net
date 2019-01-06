using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using timer = System.Timers.Timer;

namespace Device.Net
{
    public class DevicePoller
    {
        #region Fields
        private readonly timer _PollTimer;
        private readonly SemaphoreSlim _PollingSemaphoreSlim = new SemaphoreSlim(1, 1);
        private Dictionary<DeviceDefinition, IDevice> _CreatedDevices { get; } = new Dictionary<DeviceDefinition, IDevice>();
        #endregion

        public List<DeviceDefinition> DeviceDefinitions { get; } = new List<DeviceDefinition>();


        #region Events
        public event EventHandler<DeviceEventArgs> DeviceInitialized;
        public event EventHandler<DeviceEventArgs> DeviceDisconnected;
        #endregion

        #region Constructor
        public DevicePoller(IEnumerable<DeviceDefinition> registeredDevices, int pollMilliseconds)
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
                    var deviceDefinition = DeviceDefinitions.FirstOrDefault(def => def.VendorId == connectedDeviceDefinition.VendorId && def.ProductId == connectedDeviceDefinition.ProductId && def.DeviceType == connectedDeviceDefinition.DeviceType);

                    if (deviceDefinition == null) continue;

                    IDevice device = null;
                    if (_CreatedDevices.ContainsKey(deviceDefinition))
                    {
                        device = _CreatedDevices[deviceDefinition];
                    }

                    if (device == null)
                    {
                        device = DeviceManager.Current.GetDevice(deviceDefinition);
                        _CreatedDevices.Add(deviceDefinition, device);
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
                foreach (var key in _CreatedDevices.Keys)
                {
                    var device = _CreatedDevices[key];

                    if (!connectedDeviceDefinitions.Any(d => d.ProductId == key.ProductId && d.VendorId == key.VendorId))
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

                foreach(var removeDef in removeDefs)
                {
                    _CreatedDevices.Remove(removeDef);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Hid polling error", ex, nameof(DevicePoller));

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
