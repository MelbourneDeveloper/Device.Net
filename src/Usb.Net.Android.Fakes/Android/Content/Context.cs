
using Android.App;

namespace Android.Content
{
    public interface Context
    {
        Intent? RegisterReceiver(BroadcastReceiver? receiver, IntentFilter? filter);
        void UnregisterReceiver(BroadcastReceiver? receiver);
    }
}
