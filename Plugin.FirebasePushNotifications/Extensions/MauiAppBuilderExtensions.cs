﻿using Microsoft.Extensions.Logging;

#if ANDROID || IOS
using Plugin.FirebasePushNotifications.Platforms;
#endif

#if IOS
using UIKit;
using UserNotifications;
#endif

using Microsoft.Maui.LifecycleEvents;
using Plugin.FirebasePushNotifications.Internals;

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
                    var loggerFactory = ServiceLocator.Current.GetRequiredService<ILoggerFactory>();
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
                            $"launchOptions[isPushNotification={isPushNotification}, isLocalNotification={isLocalNotification}]");
                    }
                    else
                    {
                        logger.LogDebug($"FinishedLaunching");
                    }

                    var firebasePushNotification = CrossFirebasePushNotification.Current;
                    firebasePushNotification.Logger = ServiceLocator.Current.GetRequiredService<ILogger<FirebasePushNotificationManager>>();
                    firebasePushNotification.Configure(defaultOptions);

                    Firebase.Core.App.Configure();
                    Firebase.CloudMessaging.Messaging.SharedInstance.AutoInitEnabled = defaultOptions.AutoInitEnabled;

                    // In order to get OnTokenRefresh event called by firebase push notification plugin,
                    // we have to assign Messaging.SharedInstance.Delegate manually.
                    // https://github.com/CrossGeeks/FirebasePushNotificationPlugin/issues/303#issuecomment-730393259
                    Firebase.CloudMessaging.Messaging.SharedInstance.Delegate = CrossFirebasePushNotification.Current as Firebase.CloudMessaging.IMessagingDelegate;
                    UNUserNotificationCenter.Current.Delegate = CrossFirebasePushNotification.Current as IUNUserNotificationCenterDelegate;

                    return true;
                }));
#elif ANDROID
                events.AddAndroid(android => android.OnApplicationCreate(d =>
                {
                    var firebasePushNotification = CrossFirebasePushNotification.Current;
                    firebasePushNotification.Logger = ServiceLocator.Current.GetRequiredService<ILogger<FirebasePushNotificationManager>>();
                    firebasePushNotification.Configure(defaultOptions);

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
#endif
            return builder;
        }
    }
}