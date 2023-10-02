#if ANDROID || IOS
using Plugin.FirebasePushNotifications.Platforms;
#endif

#if IOS
using Firebase.CloudMessaging;
#endif

using Microsoft.Maui.LifecycleEvents;

namespace Plugin.FirebasePushNotifications
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
                events.AddiOS(iOS => iOS.FinishedLaunching((_, launchOptions) =>
                {
                    FirebasePushNotificationManager.Initialize(launchOptions, autoRegistration: false);

                    // In order to get OnTokenRefresh event called by firebase push notification plugin,
                    // we have to assign Messaging.SharedInstance.Delegate manually.
                    // https://github.com/CrossGeeks/FirebasePushNotificationPlugin/issues/303#issuecomment-730393259
                    Messaging.SharedInstance.Delegate = CrossFirebasePushNotification.Current as IMessagingDelegate;

                    return false;
                }));
#elif ANDROID
                events.AddAndroid(android => android.OnCreate((activity, intent) =>
                {
                    // TODO: Setup Android specific services
                    //FirebasePushNotificationManager.Initialize(activity, firebaseSettings)));

                    IntentHandler.CheckAndProcessIntent(activity, activity.Intent);
                }));
                events.AddAndroid(android => android.OnNewIntent((activity, intent) =>
                {
                    IntentHandler.CheckAndProcessIntent(activity, intent);
                }));
#endif
            });

            //CrossFirebasePushNotification.Current...
            return builder;
        }
    }
}