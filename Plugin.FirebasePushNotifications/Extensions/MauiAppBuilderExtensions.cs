#if ANDROID || IOS
using Plugin.FirebasePushNotifications.Platforms;
using Microsoft.Extensions.Logging;
#endif

#if IOS
using UIKit;
using UserNotifications;
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
                    var loggerFactory = MauiUIApplicationDelegate.Current.Services.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("AppDelegate");

                    if (launchOptions != null)
                    {
                        var isPushNotification =
                            launchOptions.TryGetValue(UIApplication.LaunchOptionsRemoteNotificationKey, out var pushNotificationPayload) &&
                            pushNotificationPayload != null;
                        var isLocalNotification =
                            launchOptions.TryGetValue(UIApplication.LaunchOptionsLocalNotificationKey, out var localNotificationPayload) &&
                            localNotificationPayload != null;

                        logger.LogDebug(
                            $"FinishedLaunching with " +
                            $"launchOptions[isPushNotification={isPushNotification}, " +
                            $"isLocalNotification={isLocalNotification}]");
                    }
                    else
                    {
                        logger.LogDebug($"FinishedLaunching");
                    }
                    
                    // Instead of FirebasePushNotificationManager.Initialize
                    Firebase.Core.App.Configure();
                    Firebase.CloudMessaging.Messaging.SharedInstance.AutoInitEnabled = defaultOptions.AutoInitEnabled;

                    // In order to get OnTokenRefresh event called by firebase push notification plugin,
                    // we have to assign Messaging.SharedInstance.Delegate manually.
                    // https://github.com/CrossGeeks/FirebasePushNotificationPlugin/issues/303#issuecomment-730393259
                    Firebase.CloudMessaging.Messaging.SharedInstance.Delegate = CrossFirebasePushNotification.Current as Firebase.CloudMessaging.IMessagingDelegate;
                    UNUserNotificationCenter.Current.Delegate = CrossFirebasePushNotification.Current as IUNUserNotificationCenterDelegate;

                    return false;
                }));
#elif ANDROID
                events.AddAndroid(android => android.OnApplicationCreate(d =>
                {
                    FirebasePushNotificationManager.NotificationActivityType = defaultOptions.Android.NotificationActivityType;
                    FirebasePushNotificationManager.DefaultNotificationChannelId = defaultOptions.Android.DefaultNotificationChannelId;

                    // TODO: Create StaticNotificationChannels
                    //StaticNotificationChannels.UpdateChannels(Context);

                    if (defaultOptions.AutoInitEnabled)
                    {
                        Firebase.FirebaseApp.InitializeApp(d.ApplicationContext);
                        Firebase.Messaging.FirebaseMessaging.Instance.AutoInitEnabled = defaultOptions.AutoInitEnabled;
                    }


                }));
                events.AddAndroid(android => android.OnCreate((activity, intent) =>
                {
                    Firebase.FirebaseApp.InitializeApp(activity);
                    IntentHandler.CheckAndProcessIntent(activity, activity.Intent);
                }));
                events.AddAndroid(android => android.OnNewIntent((activity, intent) =>
                {
                    IntentHandler.CheckAndProcessIntent(activity, intent);
                }));
#endif
            });

            builder.Services.AddSingleton(_ => CrossFirebasePushNotification.Current);
            return builder;
        }
    }
}