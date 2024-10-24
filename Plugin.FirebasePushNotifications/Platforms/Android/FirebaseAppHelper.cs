using Android.Content;
using Firebase;

namespace Plugin.FirebasePushNotifications
{
    internal static class FirebaseAppHelper
    {
        internal static bool IsFirebaseAppInitialized(Context context)
        {
            var isAppInitialized = false;
            var firebaseApps = FirebaseApp.GetApps(context);
            foreach (var app in firebaseApps)
            {
                if (string.Equals(app.Name, FirebaseApp.DefaultAppName))
                {
                    isAppInitialized = true;
                    break;
                }
            }

            return isAppInitialized;
        }
    }
}