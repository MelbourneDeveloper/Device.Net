using System;

namespace Device.Net
{
    public class DeviceObserver : IObserver<ConnectionEvent>
    {
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(ConnectionEvent value)
        {
            throw new NotImplementedException();
        }
    }
}
