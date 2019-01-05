using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using timer = System.Timers.Timer;

namespace Device.Net.UWP
{
    public class DevicePoller
    {
        #region Fields
        private readonly timer _PollTimer;
        private readonly SemaphoreSlim _PollingSemaphoreSlim = new SemaphoreSlim(1, 1);
        private Dictionary<VidPid, IDevice> _RegisteredDevices { get; } = new Dictionary<VidPid, IDevice>();
        #endregion

        #region Public Properties
        public uint? ProductId { get; }
        public uint? VendorId { get; }
        #endregion

        #region Constructor
        public DevicePoller(uint? productId, uint? vendorId, int pollMilliseconds)
        {
            _PollTimer = new timer(pollMilliseconds);
            _PollTimer.Elapsed += _PollTimer_Elapsed;
            _PollTimer.Start();
            ProductId = productId;
            VendorId = vendorId;
        }
        #endregion

        #region Event Handlers
        private async void _PollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await _PollingSemaphoreSlim.WaitAsync();

                var deviceInformations = await DeviceManager.Current.GetConnectedDeviceDefinitions(VendorId, ProductId);

                var connectedVidPids = new List<VidPid>();

                //Iterate through connected devices
                foreach (var deviceInformation in deviceInformations)
                {
                    var vidPid = new VidPid { Pid = deviceInformation.ProductId, Vid = deviceInformation.VendorId };

                    connectedVidPids.Add(vidPid);

                    _RegisteredDevices.TryGetValue(vidPid, out var device);

                    if (device != null)
                    {
                        if (!await device.GetIsConnectedAsync())
                        {
                            //The device is not initialized so initialize it
                            await device.InitializeAsync();
                        }
                    }
                    else
                    {
                        //TODO: Loggging. A device is connected but we're not worried about it...
                    }
                }

                //Iterate through registered devices
                foreach (var vidPid in _RegisteredDevices.Keys)
                {
                    if (!connectedVidPids.Contains(vidPid))
                    {
                        var device = _RegisteredDevices[vidPid];

                        if (await device.GetIsConnectedAsync())
                        {
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
        public void RegisterDevice(uint? vendorId, uint? productId, IDevice device)
        {
            if (!vendorId.HasValue && !productId.HasValue) throw new ArgumentNullException();

            if (device == null) throw new ArgumentNullException(nameof(device));

            var vidPid = new VidPid { Vid = vendorId, Pid = productId };

            if (_RegisteredDevices.ContainsKey(vidPid)) throw new Exception("Vendor/Product Id combination already registered");

            _RegisteredDevices.Add(vidPid, device);
        }

        public void Stop()
        {
            _PollTimer.Stop();
        }
        #endregion
    }

    internal class VidPid
    {
        public uint? Vid { get; set; }
        public uint? Pid { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is VidPid vidPid)
            {
                if (!vidPid.Pid.HasValue && !vidPid.Vid.HasValue)
                {
                    return false;
                }

                return vidPid.Vid == Vid && vidPid.Pid == Pid;
            }

            return false;
        }
    }
}
