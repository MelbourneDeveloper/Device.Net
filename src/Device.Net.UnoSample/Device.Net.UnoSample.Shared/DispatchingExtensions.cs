using System;
using System.Threading.Tasks;
using Windows.UI.Core;

#if WINDOWS_UWP
using Windows.ApplicationModel.Core;
#endif 

namespace Device.Net.UnoSample
{
    public static class DispatchingExtensions
    {
        public static async Task RunOnDispatcher(Action action) =>
#if WINDOWS_UWP
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
#else
            await CoreDispatcher.Main.RunAsync(CoreDispatcherPriority.Normal, () => action());
#endif
    }
}
