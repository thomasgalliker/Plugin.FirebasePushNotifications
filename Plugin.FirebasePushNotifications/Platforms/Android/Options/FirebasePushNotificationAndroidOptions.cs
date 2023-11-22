#if ANDROID
using Plugin.FirebasePushNotifications.Platforms.Channels;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public class FirebasePushNotificationAndroidOptions
    {
        /// <summary>
        /// The activity which handles incoming push notifications.
        /// Typically, this is <c>typeof(MainActivity)</c>.
        /// </summary>
        public virtual Type NotificationActivityType { get; set; }

        public virtual NotificationChannelRequest[] NotificationChannels { get; set; } = Array.Empty<NotificationChannelRequest>();
    }
}
#endif