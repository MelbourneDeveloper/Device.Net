using System;
using System.Collections.Generic;
using System.Linq;
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
        #endregion

        #region Public Properties
        public uint? ProductId { get; }
        public uint? VendorId { get; }
        public DeviceType? DeviceType { get; }
        public List<IDevice> RegisteredDevices { get; } = new List<IDevice>();
        #endregion

        #region Constructor
        public DevicePoller(uint? productId, uint? vendorId, DeviceType? deviceType, int pollMilliseconds)
        {
            _PollTimer = new timer(pollMilliseconds);
            _PollTimer.Elapsed += _PollTimer_Elapsed;
            _PollTimer.Start();
            ProductId = productId;
            VendorId = vendorId;
            DeviceType = deviceType;
        }
        #endregion

        #region Event Handlers
        private async void _PollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await _PollingSemaphoreSlim.WaitAsync();

                var deviceInformations = await DeviceManager.Current.GetConnectedDeviceDefinitions(VendorId, ProductId);

                foreach (var deviceInformation in deviceInformations)
                {
                    //var asdasd = RegisteredDevices.FirstOrDefault(d=>d.de)
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Hid polling error", ex, nameof(DevicePoller));

                //Throw?
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
