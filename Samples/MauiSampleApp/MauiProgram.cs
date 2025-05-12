using CommunityToolkit.Maui;
using MauiSampleApp.Services;
using MauiSampleApp.ViewModels;
using MauiSampleApp.Views;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Model.Queues;
using MauiSampleApp.Services.Logging;
using NLog.Extensions.Logging;

#if ANDROID
using Android.App;
using Firebase;
using MauiSampleApp.Platforms.Notifications;
#elif IOS
using UserNotifications;
#endif

namespace MauiSampleApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseFirebasePushNotifications(o =>
                {
                    o.AutoInitEnabled = false;

                    // Demo: Use persistent event queues instead of in-memory queues.
                    // o.QueueFactory = new PersistentQueueFactory();
#if ANDROID
                    // Demo: Configure the activity which receives the push notifications.
                    // o.Android.NotificationActivityType = typeof(MainActivity);

                    // Demo: Set the default notification importance to 'high'.
                    //       This has the same effect as the 'priority' flag sent in the data portion of the notification message.
                    // o.Android.DefaultNotificationImportance = NotificationImportance.High;

                    // Demo: Use notification channels and notification channel groups.
                    //       If no notification channels are specified, a default notification channel is created.
                    // o.Android.NotificationChannelGroups = NotificationChannelGroupSamples.GetAll().ToArray();
                    // o.Android.NotificationChannels = NotificationChannelSamples.GetAll().ToArray();
                    // o.Android.NotificationChannels = new [] { NotificationChannelSamples.Default };

                    // Demo: Notification categories are used with 'click_action'
                    //       to allow user interaction with notification messages.
                    // o.Android.NotificationCategories = NotificationCategorySamples.GetAll().ToArray();

                    // If you don't want to use the google-services.json file,
                    // you can configure Firebase programmatically
                    // o.Android.FirebaseOptions = new FirebaseOptions.Builder()
                    //     .SetApplicationId("appId")
                    //     .SetProjectId("projectId")
                    //     .SetApiKey("apiKey")
                    //     .SetGcmSenderId("senderId")
                    //     .Build();
#elif IOS
                    // You can configure iOS-specific options under o.iOS:
                    // o.iOS.FirebaseOptions = new Firebase.Core.Options("appId", "senderId");

                    // o.iOS.PresentationOptions = UNNotificationPresentationOptions.Banner;
                    // o.iOS.iOS18Workaround.Enable = true;
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("IBMPlexSans-Regular.ttf", "IBMPlexSans");
                    fonts.AddFont("IBMPlexSans-Bold.ttf", "IBMPlexSansBold");
                    fonts.AddFont("IBMPlexMono-Regular.ttf", "IBMPlexMonoRegular");
                });

            builder.Services.AddLogging(b =>
            {
                b.ClearProviders();
                b.SetMinimumLevel(LogLevel.Trace);
                b.AddDebug();
                b.AddNLog(NLogLoggerConfiguration.GetLoggingConfiguration());
                b.AddSentry(SentryConfiguration.Configure);
            });

#if ANDROID
            // Demo: Register an INotificationBuilder instance
            // in order to use a custom notification builder logic.
            //builder.Services.AddSingleton<Plugin.FirebasePushNotifications.Platforms.INotificationBuilder, CustomNotificationBuilder>();
#endif
            // Demo: Register an IPushNotificationHandler instance
            // in order to use a custom notification handler logic.
            //builder.Services.AddSingleton<IPushNotificationHandler, CustomPushNotificationHandler>();

            // Demo: Register an IFirebasePushNotificationPreferences instance
            // in order to handle preferences in your own implementation.

            // Register services used by MauiSampleApp
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainViewModel>();

            builder.Services.AddTransient<QueuesPage>();
            builder.Services.AddTransient<QueuesViewModel>();

            builder.Services.AddTransient<LogPage>();
            builder.Services.AddTransient<LogViewModel>();

            builder.Services.AddSingleton<INavigationService, MauiNavigationService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton(_ => Launcher.Default);
            builder.Services.AddSingleton(_ => Browser.Default);
            builder.Services.AddSingleton(_ => Share.Default);
            builder.Services.AddSingleton(_ => Preferences.Default);
            builder.Services.AddSingleton(_ => Email.Default);
            builder.Services.AddSingleton(_ => Clipboard.Default);
            builder.Services.AddSingleton(_ => AppInfo.Current);
            builder.Services.AddSingleton(_ => DeviceInfo.Current);
            builder.Services.AddSingleton(_ => FileSystem.Current);

            var logFileReader = new NLogFileReader(NLogLoggerConfiguration.LogFilePath);
            builder.Services.AddSingleton<ILogFileReader>(logFileReader);

            return builder.Build();
        }
    }
}