using System;

namespace Device.Net
{
    internal class DummyDisposable : IDisposable
    {
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
        }
    }
}
