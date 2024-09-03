using Microsoft.Extensions.Logging;

#if ANDROID || IOS
using Plugin.FirebasePushNotifications.Platforms;
using Plugin.FirebasePushNotifications.Platforms.Channels;
using Plugin.FirebasePushNotifications.Model.Queues;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        /// <summary>
        /// Configures Plugin.FirebasePushNotifications to use with this app.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static MauiAppBuilder UseFirebasePushNotifications(this MauiAppBuilder builder, Action<FirebasePushNotificationOptions> options = null)
        {
            var defaultOptions = new FirebasePushNotificationOptions();

            options?.Invoke(defaultOptions);

            builder.ConfigureLifecycleEvents(events =>
            {
#if IOS
                events.AddiOS(iOS => iOS.FinishedLaunching((application, launchOptions) =>
                {
                    var loggerFactory = IPlatformApplication.Current.Services.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger(typeof(MauiAppBuilderExtensions));

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
                            $"launchOptions[isPushNotification={isPushNotification}, isLocalNotification={isLocalNotification}]");
                    }
                    else
                    {
                        logger.LogDebug("FinishedLaunching");
                    }

                    var firebasePushNotification = IFirebasePushNotification.Current;

                    if (firebasePushNotification.NotificationHandler == null)
                    {
                        // Resolve IPushNotificationHandler (if not already set)
                        var pushNotificationHandler = IPlatformApplication.Current.Services.GetService<IPushNotificationHandler>();
                        firebasePushNotification.NotificationHandler = pushNotificationHandler;
                    }

                    return true;
                }));
#elif ANDROID
                // events.AddAndroid(android => android.OnApplicationCreate(d =>
                // {
                // }));
                events.AddAndroid(android => android.OnCreate((activity, intent) =>
                {
                    //Firebase.FirebaseApp.InitializeApp(activity);

                    var firebasePushNotification = IFirebasePushNotification.Current;

                    // Resolve IPushNotificationHandler (if not already set)
                    var pushNotificationHandler = IPlatformApplication.Current.Services.GetService<IPushNotificationHandler>();
                    if (pushNotificationHandler != null)
                    {
                        firebasePushNotification.NotificationHandler = pushNotificationHandler;
                    }

                    firebasePushNotification.ProcessIntent(activity, activity.Intent);
                }));
                events.AddAndroid(android => android.OnNewIntent((activity, intent) =>
                {
                    var firebasePushNotification = IFirebasePushNotification.Current;
                    firebasePushNotification.ProcessIntent(activity, intent);
                }));
#endif
            });

            // Service registrations
#if ANDROID || IOS
            builder.Services.AddSingleton(c => CrossFirebasePushNotification.Current);
            builder.Services.AddSingleton<INotificationPermissions, NotificationPermissions>();
            builder.Services.TryAddSingleton<IFirebasePushNotificationPreferences, FirebasePushNotificationPreferences>();
            builder.Services.TryAddSingleton<IPreferences>(_ => Preferences.Default);
            builder.Services.AddSingleton(defaultOptions);
#endif

#if ANDROID
            builder.Services.AddSingleton(c => NotificationChannels.Current);
            builder.Services.TryAddSingleton<INotificationBuilder, NotificationBuilder>();
#elif IOS
            builder.Services.AddSingleton<INotificationChannels, NotificationChannels>();
#endif
            return builder;
        }
    }
}