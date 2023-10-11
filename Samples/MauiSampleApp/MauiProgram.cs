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
                    o.QueueFactory = new InMemoryQueueFactory();
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

            return builder.Build();
        }
    }
}