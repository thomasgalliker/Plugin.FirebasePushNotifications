#if ANDROID

using Plugin.FirebasePushNotifications.Platforms;

namespace MauiSampleApp
{
    public class CustomNotificationBuilder : INotificationBuilder
    {
        public CustomNotificationBuilder()
        {

        }

        public void OnNotificationReceived(IDictionary<string, object> data)
        {
        }
    }
}
#endif