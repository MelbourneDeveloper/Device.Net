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
        private readonly timer _PollTimer = new timer(3000);
        private readonly SemaphoreSlim _PollingSemaphoreSlim = new SemaphoreSlim(1,1);
        #endregion

        #region Public Properties
        public uint? ProductId { get; }
        public uint? VendorId { get; }
        public DeviceType? DeviceType { get; }
        public List<IDevice> RegisteredDevices { get; } = new List<IDevice>();
        #endregion

        #region Constructor
        public DevicePoller(uint? productId, uint? vendorId, DeviceType? deviceType)
        {
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
                    try
                    {
                        //Attempt to connect and move to the next one if this one doesn't connect
                        UWPDevice.DeviceId = deviceInformation.DeviceId;
                        await UWPDevice.InitializeAsync();
                        if (await UWPDevice.GetIsConnectedAsync())
                        {
                            //Connection was successful
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Error connecting to device", ex, nameof(UWPDevicePoller));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Hid polling error", ex, nameof(UWPDevicePoller));

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

        public void RegisterDevice(IDevice device)
        {

        }
        #endregion
    }
}
