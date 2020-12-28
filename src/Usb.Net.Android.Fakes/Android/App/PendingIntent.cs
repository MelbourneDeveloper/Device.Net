using Android.Content;

namespace Android.App
{
    public interface PendingIntent
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public static PendingIntent? GetBroadcast(Context? context, int requestCode, Intent? intent, PendingIntentFlags flags) => default;
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
