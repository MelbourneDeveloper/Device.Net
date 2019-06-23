using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    public abstract class MockDeviceBase : DeviceBase, IDevice
    {
        public override ushort WriteBufferSize => 64;
        public override ushort ReadBufferSize => 64;

        protected bool _IsInitialized;

        public override bool IsInitialized => _IsInitialized;

        public void Close()
        {
        }

        public Task InitializeAsync()
        {
            _IsInitialized = true;
            return Task.FromResult(true);
        }

        private byte[] LastWrittenBuffer;

        public async override Task<byte[]> ReadAsync()
        {
            if (LastWrittenBuffer != null) return LastWrittenBuffer;

            return await Task.FromResult(new byte[64] { 1, 2, 3, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        }

        public override Task WriteAsync(byte[] data)
        {
            LastWrittenBuffer = data;
            return Task.FromResult(true);
        }
    }
}
