using Android.OS;
using Application = Android.App.Application;

namespace Plugin.FirebasePushNotifications.Utils
{
    internal static class MetadataHelper
    {
        internal static Bundle GetMetadata()
        {
            var applicationInfo = Application.Context.PackageManager.GetApplicationInfo(
                Application.Context.PackageName,
                Android.Content.PM.PackageInfoFlags.MetaData);

            return applicationInfo.MetaData;
        }
    }
}