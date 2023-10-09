#if ANDROID || IOS
#define ANDROID_OR_IOS
using Microsoft.Extensions.Logging.Abstractions;
using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Model.Queues;
using Plugin.FirebasePushNotifications.Platforms;
#endif


namespace Plugin.FirebasePushNotifications
{
    /// <summary>
    /// Cross-platform Firebase push notification.
    /// </summary>
    public class CrossFirebasePushNotification
    {
        private static Lazy<IFirebasePushNotification> Implementation = new Lazy<IFirebasePushNotification>(CreateFirebasePushNotification, LazyThreadSafetyMode.PublicationOnly);

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
        /// Current plugin implementation to use
        /// </summary>
        public static IFirebasePushNotification Current
        {
            get => Implementation.Value;
        }

        private static IFirebasePushNotification CreateFirebasePushNotification()
        {
#if ANDROID_OR_IOS
            return new FirebasePushNotificationManager(new NullLogger<FirebasePushNotificationManager>(), new InMemoryQueueFactory());
#else
            throw NotImplementedInReferenceAssembly();
#endif
        }


        private static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException(
                "This functionality is not implemented for the current platform. " +
                "You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }

        /// <summary>
        /// Clean-up implementation reference.
        /// </summary>
        public static void Dispose()
        {
            if (Implementation != null && Implementation.IsValueCreated)
            {
                Implementation = new Lazy<IFirebasePushNotification>(CreateFirebasePushNotification, LazyThreadSafetyMode.PublicationOnly);
            }
        }

        internal static void TrySetCurrent(IFirebasePushNotification instance, out IFirebasePushNotification o)
        {
            if (Implementation.IsValueCreated)
            {
                o = Implementation.Value;
            }
            else
            {
                o = instance;
                Implementation = new Lazy<IFirebasePushNotification>(() => instance);
            }
        }
    }
}