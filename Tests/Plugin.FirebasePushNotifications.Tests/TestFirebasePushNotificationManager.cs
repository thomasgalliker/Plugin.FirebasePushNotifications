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

        public void RaiseOnNotificationReceived(FirebasePushNotificationDataEventArgs eventArgs)
        {
            this.onNotificationReceived?.Invoke(this, eventArgs);
        }
    }
}
