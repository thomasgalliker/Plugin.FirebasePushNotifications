using Android.App;

namespace Plugin.FirebasePushNotifications.Utils
{
    internal static class AppHelper
    {
        internal static bool IsInForeground()
        {
            var appProcessInfo = new ActivityManager.RunningAppProcessInfo();
            ActivityManager.GetMyMemoryState(appProcessInfo);
            var isInForeground = appProcessInfo.Importance == Android.App.Importance.Foreground;
            return isInForeground;
        }
    }
}