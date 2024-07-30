using Microsoft.Extensions.Logging;

#if ANDROID || IOS
using Plugin.FirebasePushNotifications.Platforms;
using Plugin.FirebasePushNotifications.Platforms.Channels;
using Plugin.FirebasePushNotifications.Model.Queues;
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
                    if (Firebase.Core.App.DefaultInstance == null)
                    {
                        Firebase.Core.App.Configure();
                    }

                    var loggerFactory = IPlatformApplication.Current.Services.GetRequiredService<ILoggerFactory>();
                    if (defaultOptions.QueueFactory is IQueueFactory queueFactory)
                    {
                        queueFactory.LoggerFactory = loggerFactory;
                    }

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

                    if (CrossFirebasePushNotification.Current is FirebasePushNotificationManager firebasePushNotification)
                    {
                        firebasePushNotification.Logger = loggerFactory.CreateLogger<FirebasePushNotificationManager>();

                        if (firebasePushNotification.NotificationHandler == null)
                        {
                            // Resolve IPushNotificationHandler (if not already set)
                            var pushNotificationHandler = IPlatformApplication.Current.Services.GetService<IPushNotificationHandler>();
                            firebasePushNotification.NotificationHandler = pushNotificationHandler;
                        }

                        if (defaultOptions.Preferences == null)
                        {
                            // Resolve IFirebasePushNotificationPreferences (if not already set)
                            var preferences = IPlatformApplication.Current.Services.GetService<IFirebasePushNotificationPreferences>();
                            defaultOptions.Preferences = preferences;
                        }

                        firebasePushNotification.Configure(defaultOptions);
                    }

                    Firebase.CloudMessaging.Messaging.SharedInstance.AutoInitEnabled = defaultOptions.AutoInitEnabled;

                    return true;
                }));
#elif ANDROID
                events.AddAndroid(android => android.OnApplicationCreate(d =>
                {
                    var loggerFactory = IPlatformApplication.Current.Services.GetRequiredService<ILoggerFactory>();
                    if (defaultOptions.QueueFactory is IQueueFactory queueFactory)
                    {
                        queueFactory.LoggerFactory = loggerFactory;
                    }

                    var logger = loggerFactory.CreateLogger(typeof(MauiAppBuilderExtensions));

                    if (CrossFirebasePushNotification.Current is FirebasePushNotificationManager firebasePushNotification)
                    {
                        firebasePushNotification.Logger = loggerFactory.CreateLogger<FirebasePushNotificationManager>();

                        if (firebasePushNotification.NotificationBuilder == null)
                        {
                            // Resolve INotificationBuilder (if not already set)
                            var notificationBuilder = IPlatformApplication.Current.Services.GetService<INotificationBuilder>();
                            firebasePushNotification.NotificationBuilder = notificationBuilder;
                        }

                        if (firebasePushNotification.NotificationHandler == null)
                        {
                            // Resolve IPushNotificationHandler (if not already set)
                            var pushNotificationHandler = IPlatformApplication.Current.Services.GetService<IPushNotificationHandler>();
                            firebasePushNotification.NotificationHandler = pushNotificationHandler;
                        }

                        if (defaultOptions.Preferences == null)
                        {
                            // Resolve IFirebasePushNotificationPreferences (if not already set)
                            var preferences = IPlatformApplication.Current.Services.GetService<IFirebasePushNotificationPreferences>();
                            defaultOptions.Preferences = preferences;
                        }

                        firebasePushNotification.Configure(defaultOptions);
                    }

                    if (defaultOptions.AutoInitEnabled)
                    {
                        try
                        {
                            Firebase.FirebaseApp.InitializeApp(d.ApplicationContext);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "FirebaseApp.InitializeApp failed with exception. " +
                                                "Make sure the google-services.json file is present and marked as GoogleServicesJson.");
                            throw;
                        }
                    }

                    Firebase.Messaging.FirebaseMessaging.Instance.AutoInitEnabled = defaultOptions.AutoInitEnabled;
                }));
                events.AddAndroid(android => android.OnCreate((activity, intent) =>
                {
                    //Firebase.FirebaseApp.InitializeApp(activity);

                    var firebasePushNotification = CrossFirebasePushNotification.Current;
                    firebasePushNotification.ProcessIntent(activity, activity.Intent);
                }));
                events.AddAndroid(android => android.OnNewIntent((activity, intent) =>
                {
                    var firebasePushNotification = CrossFirebasePushNotification.Current;
                    firebasePushNotification.ProcessIntent(activity, intent);
                }));
#endif
            });

            // Service registrations
#if ANDROID || IOS
            builder.Services.AddSingleton(c => CrossFirebasePushNotification.Current);
            builder.Services.AddSingleton<INotificationPermissions, NotificationPermissions>();
            builder.Services.AddSingleton<IFirebasePushNotificationPreferences, FirebasePushNotificationPreferences>();
            builder.Services.AddSingleton<IPreferences>(_ => Preferences.Default);
            builder.Services.AddSingleton(defaultOptions);
#endif

#if ANDROID
            builder.Services.AddSingleton(c => NotificationChannels.Current);
            builder.Services.AddSingleton<INotificationBuilder, NotificationBuilder>();
#elif IOS
            builder.Services.AddSingleton<INotificationChannels, NotificationChannels>();
#endif
            return builder;
        }
    }
}