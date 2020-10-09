using System;
using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Flutnet;
using Flutnet.Interop.Embedding.Engine;
using FlutnetThermometer.Services;

namespace FlutnetThermometer
{
    /// <summary>
    /// Singleton class for Application wide objects. 
    /// </summary>
    public static class App
    {
        static ActivityLifecycleContextListener _lifecycleListener;
        static FlutnetBridge _bridge;

        public static Context AppContext => Application.Context;

        public static Activity CurrentActivity => _lifecycleListener?.Activity;

        public static bool Initialized { get; private set; }

        public static void Init(Application application)
        {
            if (Initialized)
                return;

            _lifecycleListener = new ActivityLifecycleContextListener();
            application.RegisterActivityLifecycleCallbacks(_lifecycleListener);
            ConfigureFlutnetRuntime();
            Initialized = true;
        }

        public static void Init(Activity activity)
        {
            if (Initialized)
                return;

            _lifecycleListener = new ActivityLifecycleContextListener { Activity = activity };
            activity.Application.RegisterActivityLifecycleCallbacks(_lifecycleListener);
            ConfigureFlutnetRuntime();
        }

        private static void ConfigureFlutnetRuntime()
        {
            try
            {
                //
                // Set the runtime using your Application KEY: the trial version has 100 native calls limit
                //
                FlutnetRuntime.Init("TRIAL-APP-KEY");

                //
                // Register the USB service in the flutnet runtime
                //
                FlutnetRuntime.RegisterPlatformService(new ThermometerServiceDroid((UsbManager)AppContext.GetSystemService(Context.UsbService), AppContext), "thermometer_service");

                Initialized = true;

            }
            catch (Exception e)
            {
                if (CurrentActivity != null)
                {
                    AlertDialog.Builder builder = new AlertDialog.Builder(CurrentActivity);
                    builder.SetTitle("Fatal Error");
                    builder.SetMessage(e.Message);
                    builder.SetCancelable(false);
                    builder.SetPositiveButton("OK", (sender, args) =>
                    {
                        Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                        Environment.Exit(0);
                    });

                    AlertDialog dialog = builder.Create();
                    dialog.Show();
                }
                else
                {
                    throw;
                }
            }
        }

        public static void ConfigureFlutnetBridge(FlutterEngine flutterEngine)
        {
            try
            {
#if DEBUG
                _bridge = new FlutnetBridge(flutterEngine, AppContext, FlutnetBridgeMode.PlatformChannel);
#else
                _bridge = new FlutnetBridge(flutterEngine, AppContext);
#endif
            }
            catch (Exception e)
            {
                if (CurrentActivity != null)
                {
                    AlertDialog.Builder builder = new AlertDialog.Builder(CurrentActivity);
                    builder.SetTitle("Error");
                    builder.SetMessage(e.Message);
                    builder.SetCancelable(false);
                    builder.SetPositiveButton("OK", (sender, args) => { });

                    AlertDialog dialog = builder.Create();
                    dialog.Show();
                }
                else
                {
                    throw;
                }
            }
        }

        public static void CleanUpFlutnetBridge()
        {
            _bridge?.Dispose();
            _bridge = null;
        }
    }

    class ActivityLifecycleContextListener : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        readonly WeakReference<Activity> _currentActivity = new WeakReference<Activity>(null);

        internal Context Context => Activity ?? Application.Context;

        internal Activity Activity
        {
            get => _currentActivity.TryGetTarget(out var a) ? a : null;
            set => _currentActivity.SetTarget(value);
        }

        void Application.IActivityLifecycleCallbacks.OnActivityCreated(Activity activity, Android.OS.Bundle savedInstanceState)
        {
            Activity = activity;
        }

        void Application.IActivityLifecycleCallbacks.OnActivityDestroyed(Activity activity)
        {
        }

        void Application.IActivityLifecycleCallbacks.OnActivityPaused(Activity activity)
        {
            Activity = activity;
        }

        void Application.IActivityLifecycleCallbacks.OnActivityResumed(Activity activity)
        {
            Activity = activity;
        }

        void Application.IActivityLifecycleCallbacks.OnActivitySaveInstanceState(Activity activity, Android.OS.Bundle outState)
        {
        }

        void Application.IActivityLifecycleCallbacks.OnActivityStarted(Activity activity)
        {
        }

        void Application.IActivityLifecycleCallbacks.OnActivityStopped(Activity activity)
        {
        }
    }
}