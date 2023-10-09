using System.Collections.Concurrent;
using Plugin.FirebasePushNotifications.Model;
using Plugin.FirebasePushNotifications.Model.Queues;

namespace Plugin.FirebasePushNotifications
{
    public class FirebasePushNotificationAndroidOptions
    {
        /// <summary>
        /// The activity which handles incoming push notifications.
        /// Typically, this is <c>typeof(MainActivity)</c>.
        /// </summary>
        public virtual Type NotificationActivityType { get; set; }

        public virtual string DefaultNotificationChannelId { get; set; }
    }

    public class FirebasePushNotificationOptions
    {
        public virtual bool AutoInitEnabled { get; set; }

        public virtual IQueueFactory QueueFactory { get; set; } = new NullQueueFactory();

#if ANDROID

        public virtual FirebasePushNotificationAndroidOptions Android { get; set; } = new FirebasePushNotificationAndroidOptions();
#endif

        public override string ToString()
        {
            return $"[{nameof(FirebasePushNotificationOptions)}: " +
                   $"{nameof(this.AutoInitEnabled)}={this.AutoInitEnabled},"
                   ;
        }
    }
}