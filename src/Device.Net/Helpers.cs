using System.Globalization;

namespace Device.Net
{
    /// <summary> 
    /// Provides helpers for all platforms. 
    /// </summary> 
    public static class Helpers
    {
        public const string WriteErrorMessage = "An error occurred while attempting to write to the device";
        public const string ReadErrorMessage = "An error occurred while attempting to read from the device";

        public static bool ContainsIgnoreCase(this string paragraph, string word)
        {
            return new CultureInfo("en-US").CompareInfo.IndexOf(paragraph, word, CompareOptions.IgnoreCase) >= 0;
        }

    }
}