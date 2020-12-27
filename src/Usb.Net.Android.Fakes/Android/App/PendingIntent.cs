using Android.Content;

namespace Android.App
{
    public interface PendingIntent
    {
        public static PendingIntent? GetBroadcast(Context? context, int requestCode, Intent? intent, PendingIntentFlags flags)
        {
            return default;
        }
    }
}
