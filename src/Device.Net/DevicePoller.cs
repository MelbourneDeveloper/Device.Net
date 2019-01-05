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
        private List<IDevice> _RegisteredDevices { get; } = new List<IDevice>();
        #endregion

        #region Events
        public event EventHandler<DeviceEventArgs> DeviceInitialized;
        public event EventHandler<DeviceEventArgs> DeviceDisconnected;
        #endregion

        #region Constructor
        public DevicePoller(IEnumerable<IDevice> registeredDevices, int pollMilliseconds)
        {
            _RegisteredDevices.AddRange(registeredDevices);
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
                foreach (var vidPid in _RegisteredDevices)
                {
                    connectedDeviceDefinitions.AddRange(await DeviceManager.Current.GetConnectedDeviceDefinitions(vidPid.VendorId, vidPid.ProductId));
                }

                //Iterate through connected devices
                foreach (var deviceInformation in connectedDeviceDefinitions)
                {
                    var connectedRegisteredDevices = _RegisteredDevices.Where(d => d.VendorId == deviceInformation.VendorId && d.ProductId == deviceInformation.ProductId).ToList();
                    if (connectedRegisteredDevices.Count > 1)
                    {
                        //TODO: Log
                        //More than one device with vid and pid...
                        break;
                    }

                    var device = connectedRegisteredDevices.FirstOrDefault();

                    if (device == null) continue;

                    if (!device.IsInitialized)
                    {
                        //The device is not initialized so initialize it
                        await device.InitializeAsync();

                        //Let listeners know a registered device was initialized
                        DeviceInitialized?.Invoke(this, new DeviceEventArgs(device));
                    }
                }

                //Iterate through registered devices
                foreach (var device in _RegisteredDevices)
                {
                    if (!connectedDeviceDefinitions.Any(d => d.ProductId == device.ProductId && d.VendorId == device.ProductId))
                    {
                        if (device.IsInitialized)
                        {
                            //Let listeners know a registered device was disconnected
                            //NOTE: let the rest of the app know before disposal so that the app can stop doing whatever it's doing.
                            DeviceDisconnected?.Invoke(this, new DeviceEventArgs(device));

                            //The device is no longer connected so disconnect it
                            device.Dispose();
                        }
                    }
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
        public void RegisterDevice(IDevice device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));

            if (_RegisteredDevices.Contains(device)) throw new Exception("Vendor/Product Id combination already registered");

            _RegisteredDevices.Add(device);
        }

        public void Stop()
        {
            _PollTimer.Stop();
        }
        #endregion
    }
}
