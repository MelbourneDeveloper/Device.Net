#if !NET45

using Device.Net.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polly;
using System;
using System.Threading.Tasks;

#if !WINDOWS_UWP
using Usb.Net;
#else
using Usb.Net.UWP;
using Hid.Net.UWP;
#endif

namespace Device.Net.UnitTests
{
    public static class StmDfuExtensions
    {
        public static int DownloadRequestLength => DownloadRequestBuffer.Length;

        private const byte DFU_GETSTATUS = 0x03;
        private const byte DFU_DNLOAD = 0x01;
        private const byte DFU_CLEARSTATUS = 0x04;
        public const int GetStatusPacketLength = 6;
        private static readonly byte[] DownloadRequestBuffer = new byte[] {
                0x21,
                0x00,
                0x00,
                0x00,
                0x08
            };

        public static Task<T> PerformControlTransferWithRetry<T>(
         this IUsbDevice usbDevice,
         Func<Task<T>> func,
         int retryCount = 3,
         int sleepDurationMilliseconds = 250)
        {
            var retryPolicy = Policy
                .Handle<ApiException>()
                .Or<ControlTransferException>()
                .WaitAndRetryAsync(
                    retryCount,
                    i => TimeSpan.FromMilliseconds(sleepDurationMilliseconds),
                    onRetryAsync: (e, t) => usbDevice.ClearStatusAsync()
                    );

            return retryPolicy.ExecuteAsync(func);
        }

        public static Task ClearStatusAsync(this IUsbDevice usbDevice)
            => usbDevice.PerformControlTransferAsync(new SetupPacket
            (
                requestType: new UsbDeviceRequestType(
                    RequestDirection.In,
                    RequestType.Class,
                    RequestRecipient.Interface),
                request: DFU_CLEARSTATUS,
                length: 0
            ));

        public static Task<TransferResult> GetStatusAsync(this IUsbDevice usbDevice)
            => usbDevice.PerformControlTransferAsync(new SetupPacket
            (
                requestType: new UsbDeviceRequestType(
                    RequestDirection.In,
                    RequestType.Class,
                    RequestRecipient.Interface),
                request: DFU_GETSTATUS,
                length: GetStatusPacketLength
            ));


        public static Task<TransferResult> SendDownloadRequestAsync(this IUsbDevice usbDevice)
            // buffer: set address pointer command (0x21), address pointer (0x08000000)
            => usbDevice.PerformControlTransferAsync(new SetupPacket
            (
                requestType: new UsbDeviceRequestType(
                    RequestDirection.Out,
                    RequestType.Class,
                    RequestRecipient.Interface),
                request: DFU_DNLOAD,
                length: (ushort)DownloadRequestBuffer.Length
            ), DownloadRequestBuffer);
    }
}

#endif
