using System;

namespace Device.Net
{
    internal static class MiscExtensions
    {
        /// <summary>
        /// Ensures that the value is not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value to check for null</param>
        /// <param name="message">The message for the exception when the value is null</param>
        /// <returns></returns>
        public static T NullCheck<T>(this T? value, string message)
            => NullCheck<T, InvalidOperationException>(value, message);

        /// <summary>
        /// Ensures that the value is not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value to check for null</param>
        /// <param name="message">The message for the exception when the value is null</param>
        /// <returns></returns>
        /// <typeparam name="TException">The type of exception to throw</typeparam>
        public static T NullCheck<T, TException>(this T? value, string message)
            where TException : Exception, new()
            => value ?? throw (Exception)Activator.CreateInstance(typeof(TException), message);

        /// <summary>
        /// Takes a potentially null value and returns the ToString(). 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns>Returns string.Empty if the value is null</returns>
        public static string ToStringOrEmpty<T>(this T? value) => value?.ToString() ?? string.Empty;

    }
}
