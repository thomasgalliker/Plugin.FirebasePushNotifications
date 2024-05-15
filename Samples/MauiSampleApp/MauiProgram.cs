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
                    o.Android.NotificationActivityType = typeof(MainActivity);
                    o.Android.NotificationChannels = NotificationChannelSamples.GetAll().ToArray();
                    //o.Android.NotificationCategories = NotificationCategorySamples.GetAll().ToArray(); // TODO:
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

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainViewModel>();

            builder.Services.AddTransient<QueuesPage>();
            builder.Services.AddTransient<QueuesViewModel>();

            builder.Services.AddTransient<LogPage>();
            builder.Services.AddTransient<LogViewModel>();

            builder.Services.AddSingleton<INavigationService, MauiNavigationService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton(_ => Share.Default);
            builder.Services.AddSingleton(_ => Preferences.Default);
            builder.Services.AddSingleton(_ => Email.Default);
            builder.Services.AddSingleton(_ => AppInfo.Current);
            builder.Services.AddSingleton(_ => DeviceInfo.Current);
            builder.Services.AddSingleton(_ => FileSystem.Current);

            var logFileReader = new NLogFileReader(NLogLoggerConfiguration.LogFilePath);
            builder.Services.AddSingleton<ILogFileReader>(logFileReader);

            return builder.Build();
        }
    }
}