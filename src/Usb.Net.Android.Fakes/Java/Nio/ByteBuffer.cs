namespace Java.Nio
{
    //
    // Summary:
    //     A buffer for bytes.
    //
    // Remarks:
    //     Android platform documentation
    //     Portions of this page are modifications based on work created and shared by the
    //     Android Open Source Project and used according to terms described in the Creative
    //     Commons 2.5 Attribution License.

    public interface ByteBuffer
    {
        sbyte Get();
#pragma warning disable IDE0060 // Remove unused parameter
        public static ByteBuffer Allocate(int capacity) => default;
        public static ByteBuffer? Wrap(byte[]? array) => default;
#pragma warning restore IDE0060 // Remove unused parameter
        Buffer? Rewind();
    }
}
