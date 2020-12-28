using Android.Content;

namespace Android.App
{
    public abstract class BroadcastReceiver
    {
        public abstract void OnReceive(Context? context, Intent? intent);
    }
}
