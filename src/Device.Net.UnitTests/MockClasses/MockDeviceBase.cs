using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    public abstract class MockDeviceBase : DeviceBase, IDevice
    {
        public override ushort WriteBufferSize => 64;
        public override ushort ReadBufferSize => 64;

        protected bool _IsInitialized;

        public override bool IsInitialized => _IsInitialized;

        protected MockDeviceBase(string deviceId, ILoggerFactory loggerFactory, ILogger logger) : base(deviceId, loggerFactory, logger)
        {

        }

        public void Close()
        {
        }

        public Task InitializeAsync()
        {
            _IsInitialized = true;
            return Task.FromResult(true);
        }

        private byte[] LastWrittenBuffer;

        public override async Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            if (LastWrittenBuffer != null)
            {
                Logger.LogTrace(new Trace(false, LastWrittenBuffer));
                return LastWrittenBuffer;
            }
            var data = new byte[] { 1, 2, 3, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Logger.LogTrace(new Trace(false, data));

            //Simulate IO delay and wait for a cancellation
            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(1);
                if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(Messages.ErrorMessageOperationCanceled);
            }

            return data;
        }

        public override async Task<uint> WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            LastWrittenBuffer = data;
            Logger.LogTrace(new Trace(true, data));
            //Simulate IO delay and wait for a cancellation
            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(1);
                if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(Messages.ErrorMessageOperationCanceled);
            }

            return (uint)data.Length;
        }
    }
}
