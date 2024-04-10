#if ANDROID || IOS
#define ANDROID_OR_IOS
using Plugin.FirebasePushNotifications.Platforms;
#endif


namespace Plugin.FirebasePushNotifications
{
    /// <summary>
    /// Cross-platform Firebase push notification.
    /// </summary>
    public static class CrossFirebasePushNotification
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
            return new FirebasePushNotificationManager();
#else
            throw Exceptions.NotImplementedInReferenceAssembly();
#endif
        }
    }
}