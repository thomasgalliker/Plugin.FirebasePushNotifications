#if ANDROID || IOS
#define ANDROID_OR_IOS
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Platforms;
#endif

namespace Plugin.FirebasePushNotifications
{
    public static class CrossNotificationPermissions
    {
        private static readonly Lazy<INotificationPermissions> Implementation = new Lazy<INotificationPermissions>(CreateNotificationPermissions, LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Gets the singleton instance of <see cref="INotificationPermissions"/>.
        /// </summary>
        public static INotificationPermissions Current
        {
            get => Implementation.Value;
        }

        private static INotificationPermissions CreateNotificationPermissions()
        {
#if ANDROID_OR_IOS
#if IOS
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<NotificationPermissions>>();
#endif
            return new NotificationPermissions(
#if IOS
                logger
#endif
                );
#else
            throw Exceptions.NotImplementedInReferenceAssembly();
#endif
        }
    }
}