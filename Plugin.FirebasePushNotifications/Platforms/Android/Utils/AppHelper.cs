using Android.App;
using Android.Content;
using Android.Gms.Common.Util;
using Android.OS;

namespace Plugin.FirebasePushNotifications.Utils
{
    internal static class AppHelper
    {
        internal static bool IsAppForeground()
        {
            var appProcessInfo = new ActivityManager.RunningAppProcessInfo();
            ActivityManager.GetMyMemoryState(appProcessInfo);
            var isInForeground = appProcessInfo.Importance == Importance.Foreground;
            return isInForeground;
        }

        internal static bool IsAppForeground(Context context)
        {
            var keyguardManager = (KeyguardManager)context.GetSystemService(Context.KeyguardService);
            if (keyguardManager.InKeyguardRestrictedInputMode())
            {
                return false; // Screen is off or lock screen is showing
            }

            // Screen is on and unlocked, now check if the process is in the foreground

            if (!PlatformVersion.IsAtLeastLollipop)
            {
                // Before L the process has IMPORTANCE_FOREGROUND while it executes BroadcastReceivers.
                // As soon as the service is started the BroadcastReceiver should stop.
                // UNFORTUNATELY the system might not have had the time to downgrade the process
                // (this is happening consistently in JellyBean).
                // With SystemClock.sleep(10) we tell the system to give a little bit more of CPU
                // to the main thread (this code is executing on a secondary thread) allowing the
                // BroadcastReceiver to exit the onReceive() method and downgrade the process priority.
                SystemClock.Sleep(10);
            }

            var pid = Process.MyPid();
            var am = (ActivityManager)context.GetSystemService(Context.ActivityService);
            var appProcesses = am.RunningAppProcesses;
            if (appProcesses != null)
            {
                foreach (var process in appProcesses)
                {
                    if (process.Pid == pid)
                    {
                        return process.Importance == Importance.Foreground;
                    }
                }
            }

            return false;
        }
    }
}