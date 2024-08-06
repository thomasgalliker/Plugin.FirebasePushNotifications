#if ANDROID

using AndroidX.Core.App;
using Plugin.FirebasePushNotifications.Platforms;

namespace MauiSampleApp
{
    public class CustomNotificationBuilder : INotificationBuilder
    {
        public CustomNotificationBuilder()
        {

        }

        public bool ShouldHandleNotificationReceived(IDictionary<string, object> data)
        {
            return false;
        }

        public void OnNotificationReceived(IDictionary<string, object> data)
        {
        }

        public void OnBuildNotification(NotificationCompat.Builder notificationBuilder, IDictionary<string, object> data)
        {
        }
    }
}
#endif