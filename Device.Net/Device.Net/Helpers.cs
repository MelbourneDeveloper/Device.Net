using System.Linq;

namespace Device.Net
{
    /// <summary> 
    /// Provides helpers for all platforms. 
    /// </summary> 
    public class Helpers
    {
        public const string WriteErrorMessage = "An error occurred while attempting to write to the device";
        public const string ReadErrorMessage = "An error occurred while attempting to read from the device";


        #region Public Methods 
        /// <summary> 
        /// TODO: why is this necessary on Windows? Why do we get 65 bytes back from the Trezor for example instead of 64? 
        /// Work on performance here 
        /// </summary> 
        public static byte[] RemoveFirstByte(byte[] bytes)
        {
            return GetRange(bytes, 1, bytes.Length - 1);
        }
        #endregion

        #region Private Methods 
        /// <summary> 
        /// Horribly inefficient array thing 
        /// </summary> 
        private static byte[] GetRange(byte[] bytes, int startIndex, int length)
        {
            return bytes.ToList().GetRange(startIndex, length).ToArray();
        }
        #endregion
    }
}