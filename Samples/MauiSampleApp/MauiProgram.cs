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
                    o.QueueFactory = new PersistentQueueFactory();
#if ANDROID
                    // o.Android.NotificationActivityType = typeof(MainActivity);
                    // o.Android.NotificationChannels = NotificationChannelSamples.GetAll().ToArray();
                    // o.Android.NotificationCategories = NotificationCategorySamples.GetAll().ToArray();
#elif IOS
                    // o.iOS.PresentationOptions = UNNotificationPresentationOptions.Banner;
                    // o.iOS.iOS18Workaround.Enable = true;
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddLogging(b =>
            {
                b.ClearProviders();
                b.SetMinimumLevel(LogLevel.Trace);
                b.AddDebug();
                b.AddNLog();
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