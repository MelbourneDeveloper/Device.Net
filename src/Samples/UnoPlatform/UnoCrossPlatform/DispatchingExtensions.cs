using System;
using System.Threading.Tasks;
using Windows.UI.Core;

#if NETFX_CORE
using Windows.ApplicationModel.Core;
#endif 

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
