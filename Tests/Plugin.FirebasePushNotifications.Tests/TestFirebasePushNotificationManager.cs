using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Model.Queues;
using Plugin.FirebasePushNotifications.Platforms;

namespace Plugin.FirebasePushNotifications.Tests
{
    public class TestFirebasePushNotificationManager : FirebasePushNotificationManagerBase
    {
        public TestFirebasePushNotificationManager(
            ILogger<FirebasePushNotificationManager> logger, IQueueFactory queueFactory)
            : base(logger, queueFactory)
        {
        }

        public void RaiseOnTokenRefresh(string token)
        {
            this.HandleTokenRefresh(token);
        }

        public new void HandleNotificationReceived(IDictionary<string, object> data)
        {
            base.HandleNotificationReceived(data);
        }

        public void HandleNotificationDeleted(Dictionary<string, object> data)
        {
            base.HandleNotificationDeleted(data);
        }
        
        public void HandleNotificationOpened(Dictionary<string, object> data, string identifier, NotificationCategoryType notificationCategoryType)
        {
            base.HandleNotificationOpened(data, identifier, notificationCategoryType);
        }
        
        public void HandleNotificationAction(Dictionary<string, object> data, string identifier, NotificationCategoryType notificationCategoryType)
        {
            base.HandleNotificationAction(data, identifier, notificationCategoryType);
        }
    }
}
