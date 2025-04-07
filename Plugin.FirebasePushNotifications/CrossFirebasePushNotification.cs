#if ANDROID || IOS
#define ANDROID_OR_IOS
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Platforms;
#endif


namespace Plugin.FirebasePushNotifications
{
    /// <summary>
    /// Cross-platform Firebase push notification.
    /// </summary>
    public static class CrossFirebasePushNotification
    {
        private static readonly Lazy<IFirebasePushNotification> Implementation = new Lazy<IFirebasePushNotification>(CreateFirebasePushNotification, LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Gets if the plugin is supported on the current platform.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
#if ANDROID_OR_IOS
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Gets the singleton instance of <see cref="IFirebasePushNotification"/>.
        /// </summary>
        public static IFirebasePushNotification Current
        {
            get => Implementation.Value;
        }

        private static IFirebasePushNotification CreateFirebasePushNotification()
        {
#if ANDROID_OR_IOS
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<FirebasePushNotificationManager>>();
            var loggerFactory = IPlatformApplication.Current.Services.GetRequiredService<ILoggerFactory>();
            var options = IPlatformApplication.Current.Services.GetService<FirebasePushNotificationOptions>();
            var pushNotificationHandler = IPlatformApplication.Current.Services.GetService<IPushNotificationHandler>();
            var preferences = IPlatformApplication.Current.Services.GetService<IFirebasePushNotificationPreferences>();

#if ANDROID
            var notificationChannels = IPlatformApplication.Current.Services.GetService<INotificationChannels>();
            var notificationBuilder = IPlatformApplication.Current.Services.GetService<INotificationBuilder>();
#endif

            return new FirebasePushNotificationManager(
                logger,
                loggerFactory,
                options,
                pushNotificationHandler,
                preferences
#if ANDROID
                ,
                notificationChannels,
                notificationBuilder
#endif
                );
#else
            throw Exceptions.NotImplementedInReferenceAssembly();
#endif
        }
    }
}