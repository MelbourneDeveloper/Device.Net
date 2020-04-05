using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Device.Net
{
    public class DeviceObserver : IObserver<ConnectionEventArgs>
    {
        #region Fields
        private readonly object _lock = new object();
        #endregion

        #region Events
        public event EventHandler<ConnectionEventArgs> ConnectionEventOccurred;
        #endregion

        #region Public Properties
        public ReadOnlyCollection<IDevice> Devices { get; private set; } = new ReadOnlyCollection<IDevice>(new List<IDevice>());
        #endregion

        #region Implementation
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(ConnectionEventArgs value)
        {
            if (value == null) return;

            var addFunc = new Func<ReadOnlyCollection<IDevice>>(() =>
            {
                if (Devices.Contains(value.Device)) return Devices;

                var tempDevices = Devices.ToList();

                tempDevices.Add(value.Device);

                var devices = new ReadOnlyCollection<IDevice>(tempDevices);

                System.Diagnostics.Debug.WriteLine($"Added {value.Device.DeviceId}. Count: {devices.Count}");

                return devices;
            });

            var removeFunc = new Func<ReadOnlyCollection<IDevice>>(() =>
            {
                //value.Device.Dispose();

                if (!Devices.Contains(value.Device)) return Devices;

                var tempDevices = Devices.ToList();

                tempDevices.Remove(value.Device);

                var devices = new ReadOnlyCollection<IDevice>(tempDevices);

                System.Diagnostics.Debug.WriteLine($"Removed {value.Device.DeviceId}. Count: {devices.Count}");

                return devices;
            });

            if (value.IsDisconnection)
            {
                SetDevicesSafe(removeFunc);
                ConnectionEventOccurred?.Invoke(this, value);
            }
            else
            {
                SetDevicesSafe(addFunc);
                ConnectionEventOccurred?.Invoke(this, value);
            }
        }
        #endregion

        #region Private Methods
        private void SetDevicesSafe(Func<ReadOnlyCollection<IDevice>> getDevicesFunc)
        {
            lock (_lock)
            {
                Devices = getDevicesFunc();
            }
        }
        #endregion
    }
}
