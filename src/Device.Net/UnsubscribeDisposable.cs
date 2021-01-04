using System;

namespace Device.Net
{
    public class UnsubscribeDisposable : IDisposable
    {
        private readonly Action _unsubscribe;

        public UnsubscribeDisposable(Action unsubscribe) => _unsubscribe = unsubscribe;

        public void Dispose() => _unsubscribe();
    }
}
