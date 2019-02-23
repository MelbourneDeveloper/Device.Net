using Device.Net.UWP;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Foundation;
using Windows.Storage;
using h = Hid.Net;

namespace Hid.Net.UWP
{
    public class UWPHidDevice : UWPDeviceBase<HidDevice>, h.IHidDevice
    {
        #region Public Properties
        public bool DataHasExtraByte { get; set; } = true;
        #endregion

        #region Public Override Properties
        /// <summary>
        /// TODO: These vales are completely wrong and not being used anyway...
        /// </summary>
        public override ushort WriteBufferSize => 64;
        /// <summary>
        /// TODO: These vales are completely wrong and not being used anyway...
        /// </summary>
        public override ushort ReadBufferSize => 64;
        #endregion

        #region Event Handlers
        private void _HidDevice_InputReportReceived(HidDevice sender, HidInputReportReceivedEventArgs args)
        {
            HandleDataReceived(InputReportToBytes(args));
        }
        #endregion

        #region Constructors
        public UWPHidDevice()
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
                bytes = RemoveFirstByte(bytes);
            }

            return bytes;
        }

        public override async Task InitializeAsync()
        {
            //TODO: Put a lock here to stop reentrancy of multiple calls

            if (Disposed) throw new Exception(DeviceDisposedErrorMessage);

            Log("Initializing Hid device", null);

            await GetDeviceAsync(DeviceId);

            if (ConnectedDevice != null)
            {
                ConnectedDevice.InputReportReceived += _HidDevice_InputReportReceived;
            }
            else
            {
                throw new Exception($"The device {DeviceId} failed to initialize");
            }
        }

        protected override IAsyncOperation<HidDevice> FromIdAsync(string id)
        {
            return GetHidDevice(id);
        }
        #endregion

        #region Public Methods
        public override Task WriteAsync(byte[] data)
        {
            return WriteAsync(data, 0);
        }

        public async Task WriteAsync(byte[] data, byte reportId)
        {
            byte[] bytes;
            if (DataHasExtraByte)
            {
                bytes = new byte[data.Length + 1];
                Array.Copy(data, 0, bytes, 1, data.Length);
                bytes[0] = reportId;
            }
            else
            {
                bytes = data;
            }

            var buffer = bytes.AsBuffer();
            var outReport = ConnectedDevice.CreateOutputReport();
            outReport.Data = buffer;

            try
            {
                var operation = ConnectedDevice.SendOutputReportAsync(outReport);
                await operation.AsTask();
                Tracer?.Trace(false, bytes);
            }
            catch (ArgumentException ex)
            {
                //TODO: Check the string is nasty. Validation on the size of the array being sent should be done earlier anyway
                if (ex.Message == "Value does not fall within the expected range.")
                {
                    throw new Exception("It seems that the data being sent to the device does not match the accepted size. Have you checked DataHasExtraByte?", ex);
                }
                throw;
            }
        }
        #endregion

        #region Public Static Methods
        public static IAsyncOperation<HidDevice> GetHidDevice(string id)
        {
            return HidDevice.FromIdAsync(id, FileAccessMode.ReadWrite);
        }
        #endregion
    }
}
