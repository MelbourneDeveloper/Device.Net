#if NETCOREAPP3_1

using Java.Nio;

namespace Device.Net.UnitTests
{
    public class TrezorResponseByteBuffer : ByteBuffer
    {
        private int _currentPosition;
        private readonly byte[] TrezorResponse = new byte[] { 63, 35, 35, 0, 17, 0, 0, 0, 194, 10, 9, 116, 114, 101, 122, 111, 114, 46, 105, 111, 16, 1, 24, 9, 32, 3, 50, 24, 51, 66, 69, 65, 55, 66, 50, 55, 50, 55, 66, 49, 55, 57, 50, 52, 67, 56, 67, 70, 68, 56, 53, 48, 56, 1, 64, 0, 74, 5, 101, 110, 45, 85, 83, 82 };

        public sbyte Get()
        {
            var returnValue = (sbyte)TrezorResponse[_currentPosition];
            _currentPosition++;
            return returnValue;
        }

        public Buffer Rewind() { _currentPosition = 0; return default; }
    }
}
#endif
