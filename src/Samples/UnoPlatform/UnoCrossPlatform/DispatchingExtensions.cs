using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace UnoCrossPlatform
{
    public static class DispatchingExtensions
    {
        public static async Task RunOnDispatcher(Action action) =>
#if NETFX_CORE
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
#else
            await CoreDispatcher.Main.RunAsync(CoreDispatcherPriority.Normal, () => action());
#endif

    }
}
