using MauiSampleApp.Services;
using MauiSampleApp.ViewModels;
using MauiSampleApp.Views;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Model.Queues;

namespace MauiSampleApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseFirebasePushNotifications(o =>
                {
                    o.AutoInitEnabled = false;
                    o.QueueFactory = new PersistentQueueFactory();
#if ANDROID
                    o.Android.DefaultNotificationChannelId = "general";
                    o.Android.NotificationActivityType = typeof(MainActivity);
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

            return builder.Build();
        }
    }
}