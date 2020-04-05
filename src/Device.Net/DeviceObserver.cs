using System;

namespace Device.Net
{
    public class DeviceObserver : IObserver<ConnectionInfo>
    {
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(ConnectionInfo value)
        {
            throw new NotImplementedException();
        }
    }
}
