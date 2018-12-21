using System;
using System.Timers;
using Device.Net;

namespace Hid.Net.UWP
{
    public class UWPHidDevicePoller
    {
        #region Fields
        private Timer _PollTimer = new Timer(3000);
        private bool _IsPolling;
        #endregion

        #region Public Properties
        public int ProductId { get; }
        public int VendorId { get; }
        public UWPHidDevice UWPHidDevice { get; private set; }
        #endregion

        #region Constructor
        public UWPHidDevicePoller(int productId, int vendorId, UWPHidDevice uwpHidDevice)
        {
            _PollTimer.Elapsed += _PollTimer_Elapsed;
            _PollTimer.Start();
            ProductId = productId;
            VendorId = vendorId;
            UWPHidDevice = uwpHidDevice;
        }
        #endregion

        #region Event Handlers
        private async void _PollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_IsPolling)
            {
                return;
            }

            _IsPolling = true;

            try
            {
                var foundDeviceInformations = await UWPHelpers.GetDevicesByProductAndVendorAsync(VendorId, ProductId);

                foreach (var deviceInformation in foundDeviceInformations)
                {
                    try
                    {

                        //foreach (var keyValuePair in deviceInformation.Properties)
                        //{
                        //    System.Diagnostics.Debug.WriteLine($"Key: {keyValuePair.Key} Value: {keyValuePair.Value}");
                        //}

                        //Attempt to connect and move to the next one if this one doesn't connect
                        UWPHidDevice.DeviceId = deviceInformation.Id;
                        await UWPHidDevice.InitializeAsync();
                        if (await UWPHidDevice.GetIsConnectedAsync())
                        {
                            //Connection was successful
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Error connecting to device", ex, nameof(UWPHidDevicePoller));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Hid polling error", ex, nameof(UWPHidDevicePoller));
            }

            _IsPolling = false;
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
