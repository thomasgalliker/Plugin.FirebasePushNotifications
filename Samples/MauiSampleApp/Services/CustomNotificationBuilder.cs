#if ANDROID

using AndroidX.Core.App;
using Plugin.FirebasePushNotifications.Platforms;

namespace MauiSampleApp
{
    /// <summary>
    /// Custom implementation of INotificationBuilder.
    /// You can implement the interface INotificationBuilder or extend the default logic given in NotificationBuilder.
    /// </summary>
    public class CustomNotificationBuilder : Plugin.FirebasePushNotifications.Platforms.INotificationBuilder
    {
        public CustomNotificationBuilder()
        {
        }

        public bool ShouldHandleNotificationReceived(IDictionary<string, object> data)
        {
            // TODO: Evaluate if we need to display the received notification data in a notification popup.
            return true;
        }

        public void OnNotificationReceived(IDictionary<string, object> data)
        {
            // TODO: Use Android.App.NotificationManager here to display a notification popup.
        }
    }
}
#endif