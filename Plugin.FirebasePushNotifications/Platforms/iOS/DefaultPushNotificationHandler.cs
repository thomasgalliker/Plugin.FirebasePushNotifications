using System.Diagnostics;

namespace Plugin.FirebasePushNotifications
{
    public class DefaultPushNotificationHandler : IPushNotificationHandler
    {
        public virtual void OnError(string error)
        {
            Debug.WriteLine($"{nameof(DefaultPushNotificationHandler)} - OnError - {error}");
        }

        public virtual void OnOpened(NotificationResponse response)
        {
            Debug.WriteLine($"{nameof(DefaultPushNotificationHandler)} - OnOpened");
        }

        public virtual void OnReceived(IDictionary<string, object> parameters)
        {
            Debug.WriteLine($"{nameof(DefaultPushNotificationHandler)} - OnReceived");
        }
    }
}
