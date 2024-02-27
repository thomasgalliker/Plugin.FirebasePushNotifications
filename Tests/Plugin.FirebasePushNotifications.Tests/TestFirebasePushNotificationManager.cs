using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Platforms;

namespace Plugin.FirebasePushNotifications.Tests
{
    public class TestFirebasePushNotificationManager : FirebasePushNotificationManagerBase
    {
        public TestFirebasePushNotificationManager(ILogger<FirebasePushNotificationManager> logger, FirebasePushNotificationOptions options)
        {
            this.Logger = logger;
            this.Configure(options);
        }
    }
}
