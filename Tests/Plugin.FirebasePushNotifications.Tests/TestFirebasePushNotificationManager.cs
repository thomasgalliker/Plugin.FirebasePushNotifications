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

        public string SdkVersion { get; } = "1.0.0-test";

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

        public Task SubscribeTopicAsync(string topic)
        {
            return Task.CompletedTask;
        }

        public Task SubscribeTopicsAsync(string[] topics)
        {
            return Task.CompletedTask;
        }

        public Task UnregisterForPushNotificationsAsync()
        {
            return Task.CompletedTask;
        }

        public Task UnsubscribeAllTopicsAsync()
        {
            return Task.CompletedTask;
        }

        public Task UnsubscribeTopicAsync(string topic)
        {
            return Task.CompletedTask;
        }

        public Task UnsubscribeTopicsAsync(string[] topics)
        {
            return Task.CompletedTask;
        }
    }
}
