using Device.Net;
using Device.Net.UWP;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Foundation;
using Windows.Storage;

namespace Hid.Net.UWP
{
    public class UWPHidDevice : UWPDeviceBase<HidDevice>
    {
        #region Public Properties
        public bool DataHasExtraByte { get; set; } = true;
        #endregion

        #region Event Handlers
        private void _HidDevice_InputReportReceived(HidDevice sender, HidInputReportReceivedEventArgs args)
        {
            HandleDataReceived(InputReportToBytes(args));
        }
        #endregion

        #region Constructors
        public UWPHidDevice() : base()
        {
        }

        public UWPHidDevice(string deviceId) : base(deviceId)
        {
        }
        #endregion

        #region Private Methods
        private byte[] InputReportToBytes(HidInputReportReceivedEventArgs args)
        {
            byte[] bytes;
            using (var stream = args.Report.Data.AsStream())
            {
                bytes = new byte[args.Report.Data.Length];
                stream.Read(bytes, 0, (int)args.Report.Data.Length);
            }

            if (DataHasExtraByte)
            {
                bytes = Helpers.RemoveFirstByte(bytes);
            }

            return bytes;
        }

        public override async Task InitializeAsync()
        {
            //TODO: Put a lock here to stop reentrancy of multiple calls

            //TODO: Dispose but this seems to cause initialization to never occur
            //Dispose();

            Logger.Log("Initializing Hid device", null, nameof(UWPHidDevice));

            await GetDevice(DeviceId);

            if (_ConnectedDevice != null)
            {
                _ConnectedDevice.InputReportReceived += _HidDevice_InputReportReceived;
                RaiseConnected();
            }
        }

        protected override IAsyncOperation<HidDevice> FromIdAsync(string id)
        {
            return HidDevice.FromIdAsync(id, FileAccessMode.ReadWrite);
        }
        #endregion

        #region Public Methods

        public override async Task WriteAsync(byte[] data)
        {
            byte[] bytes;
            if (DataHasExtraByte)
            {
                bytes = new byte[data.Length + 1];
                Array.Copy(data, 0, bytes, 1, data.Length);
                bytes[0] = 0;
            }
            else
            {
                bytes = data;
            }

            var buffer = bytes.AsBuffer();
            var outReport = _ConnectedDevice.CreateOutputReport();
            outReport.Data = buffer;
            IAsyncOperation<uint> operation = null;

            try
            {
                operation = _ConnectedDevice.SendOutputReportAsync(outReport);
            }
            catch (ArgumentException ex)
            {
                //TODO: Check the string is nasty. Validation on the size of the array being sent should be done earlier anyway
                if (ex.Message == "Value does not fall within the expected range.")
                {
                    throw new Exception("It seems that the data being sent to the device does not match the accepted size. Have you checked DataHasExtraByte?", ex);
                }
            }

            Tracer?.Trace(false, bytes);

            await operation.AsTask();
        }
        #endregion
    }
}
