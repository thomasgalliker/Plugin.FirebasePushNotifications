using Microsoft.Extensions.Logging;

#if ANDROID || IOS
using Plugin.FirebasePushNotifications.Platforms;
#endif

#if IOS
using UIKit;
using UserNotifications;
#endif

using Microsoft.Maui.LifecycleEvents;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Plugin.FirebasePushNotifications.Model.Queues;

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

/* Unmerged change from project 'Plugin.FirebasePushNotifications (net7.0-android)'
Before:
                    }
                    
                    // Instead of FirebasePushNotificationManager.Initialize
After:
                    }

                    // Instead of FirebasePushNotificationManager.Initialize
*/
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
                    var firebasePushNotification = MauiApplication.Current.Services.GetService<IFirebasePushNotification>();

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

                    var firebasePushNotification = MauiApplication.Current.Services.GetService<IFirebasePushNotification>();
                    firebasePushNotification.ProcessIntent(activity, activity.Intent);
                }));
                events.AddAndroid(android => android.OnNewIntent((activity, intent) =>
                {
                    var firebasePushNotification = MauiApplication.Current.Services.GetService<IFirebasePushNotification>();
                    firebasePushNotification.ProcessIntent(activity, intent);
                }));
#endif
            });

            // Service registrations
#if ANDROID || IOS
            builder.Services.TryAddSingleton<IQueueFactory, InMemoryQueueFactory>();
            builder.Services.AddSingleton(c =>
            {
                var firebasePushNotificationManager = new FirebasePushNotificationManager(
                    c.GetRequiredService<ILogger<FirebasePushNotificationManager>>(),
                    c.GetRequiredService<IQueueFactory>());

                CrossFirebasePushNotification.TrySetCurrent(firebasePushNotificationManager, out var current);

                return current;
            });
#endif
            return builder;
        }
    }
}