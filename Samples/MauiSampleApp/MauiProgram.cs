using CommunityToolkit.Maui;
using MauiSampleApp.Services;
using MauiSampleApp.ViewModels;
using MauiSampleApp.Views;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Model.Queues;

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
            });

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainViewModel>();

            builder.Services.AddSingleton<QueuesPage>();
            builder.Services.AddSingleton<QueuesViewModel>();

            builder.Services.AddSingleton<INavigationService, MauiNavigationService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton(_ => Share.Default);
            builder.Services.AddSingleton(_ => Preferences.Default);

            return builder.Build();
        }
    }
}