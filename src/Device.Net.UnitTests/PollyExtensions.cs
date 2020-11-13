#if !NET45

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Polly;
using Device.Net.Exceptions;

#if !WINDOWS_UWP
using Usb.Net;
#else
using Usb.Net.UWP;
using Hid.Net.UWP;
#endif

namespace Device.Net.UnitTests
{
    public static class PollyExtensions
    {
        private const byte DFU_CLEARSTATUS = 0x04;

        public static Task PerformControlTransferWithRetry(this IUsbDevice usbDevice, Func<Task> func, int retryCount = 3, int sleepDurationMilliseconds = 250)
        {
            var clearStatusSetupPacket = new SetupPacket
            (
                requestType: new UsbDeviceRequestType(
                    RequestDirection.In,
                    RequestType.Class,
                    RequestRecipient.Interface),
                request: DFU_CLEARSTATUS,
                length: 0
            );

            var retryPolicy = Policy
                .Handle<ApiException>()
                .Or<ControlTransferException>()
                .Or<AssertFailedException>()
                .WaitAndRetryAsync(
                    retryCount,
                    i => TimeSpan.FromMilliseconds(sleepDurationMilliseconds),
                    onRetry: (e, t) => usbDevice.PerformControlTransferAsync(clearStatusSetupPacket)
                    );

            return retryPolicy.ExecuteAsync(func);
        }
    }
}

#endif
