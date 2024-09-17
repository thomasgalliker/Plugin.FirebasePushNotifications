using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Platforms;

namespace Plugin.FirebasePushNotifications.Tests
{
    /// <summary>
    /// Implementation of <see cref="IFirebasePushNotification"/>
    /// for unit testing.
    /// </summary>
    public class TestFirebasePushNotificationManager : FirebasePushNotificationManagerBase, IFirebasePushNotification
    {
        public TestFirebasePushNotificationManager(
            ILogger<IFirebasePushNotification> logger,
            ILoggerFactory loggerFactory,
            FirebasePushNotificationOptions options,
            IPushNotificationHandler pushNotificationHandler,
            IFirebasePushNotificationPreferences preferences)
            : base(logger, loggerFactory, options, pushNotificationHandler, preferences)
        {
        }

        public string Token { get; }

        public Task RegisterForPushNotificationsAsync()
        {
            return Task.CompletedTask;
        }

        public void RemoveNotification(int id)
        {
        }

        public void RemoveNotification(string tag, int id)
        {
        }

        public void SubscribeTopic(string topic)
        {
        }

        public void SubscribeTopics(string[] topics)
        {
        }

        public Task UnregisterForPushNotificationsAsync()
        {
            return Task.CompletedTask;
        }

        public void UnsubscribeAllTopics()
        {
        }

        public void UnsubscribeTopic(string topic)
        {
        }

        public void UnsubscribeTopics(string[] topics)
        {
        }
    }
}
