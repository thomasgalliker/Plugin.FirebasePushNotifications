using System.Diagnostics;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public class DefaultPushNotificationHandler : IPushNotificationHandler
    {
        public virtual void OnError(string error)
        {
            Debug.WriteLine($"{nameof(DefaultPushNotificationHandler)} - OnError - {error}");
        }

        public virtual void OnOpened(IDictionary<string, object> parameters, string identifier, NotificationCategoryType notificationCategoryType)
        {
            Debug.WriteLine($"{nameof(DefaultPushNotificationHandler)} - OnOpened");
        }

        public virtual void OnReceived(IDictionary<string, object> parameters)
        {
            Debug.WriteLine($"{nameof(DefaultPushNotificationHandler)} - OnReceived");
        }
    }
}
