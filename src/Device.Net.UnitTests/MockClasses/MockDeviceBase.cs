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

        protected MockDeviceBase(string deviceId, ILogger logger, ITracer tracer) : base(deviceId, logger, tracer)
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

        public override async Task<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            if (LastWrittenBuffer != null)
            {
                Tracer?.Trace(false, LastWrittenBuffer);
                return LastWrittenBuffer;
            }
            var data = new byte[] { 1, 2, 3, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Tracer?.Trace(false, data);

            //Simulate IO delay and wait for a cancellation
            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(1);
                if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(Messages.ErrorMessageOperationCanceled);
            }

            return data;
        }

        public override async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            LastWrittenBuffer = data;
            Tracer?.Trace(true, data);
            //Simulate IO delay and wait for a cancellation
            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(1);
                if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(Messages.ErrorMessageOperationCanceled);
            }
        }
    }
}
