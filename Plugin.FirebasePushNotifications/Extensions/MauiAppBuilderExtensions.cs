#if ANDROID || IOS
using Plugin.FirebasePushNotifications.Platforms;
#endif

using Microsoft.Maui.LifecycleEvents;

namespace Plugin.FirebasePushNotifications.Extensions
{
    public static class MauiAppBuilderExtensions
    {
        public static MauiAppBuilder UseFirebasePushNotifications(this MauiAppBuilder builder, Action<FirebasePushNotificationOptions> options = null)
        {
            var defaultOptions = new FirebasePushNotificationOptions();

            options?.Invoke(defaultOptions);

            builder.ConfigureLifecycleEvents(events =>
            {
#if IOS
                events.AddiOS(iOS => iOS.FinishedLaunching((_, _) =>
                {
                    // TODO: Setup iOS specific services
                    //FirebasePushNotificationManager.Initialize(options);
                    return false;
                }));
#elif ANDROID
                events.AddAndroid(android => android.OnCreate((activity, _) =>
                {
                    // TODO: Setup Android specific services
                    //FirebasePushNotificationManager.Initialize(activity, firebaseSettings)));
                }));
#endif
            });

            //CrossFirebasePushNotification.Current...
            return builder;
        }
    }
}