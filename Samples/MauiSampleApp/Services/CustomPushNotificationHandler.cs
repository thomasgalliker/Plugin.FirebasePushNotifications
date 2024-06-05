using Plugin.FirebasePushNotifications;

namespace MauiSampleApp.Services
{
    public class CustomPushNotificationHandler : IPushNotificationHandler
    {
        public CustomPushNotificationHandler()
        {
        }

        public void OnOpened(IDictionary<string, object> parameters, NotificationAction notificationAction, NotificationCategoryType notificationCategoryType)
        {
        }

        public void OnReceived(IDictionary<string, object> parameters)
        {
        }
    }
}
