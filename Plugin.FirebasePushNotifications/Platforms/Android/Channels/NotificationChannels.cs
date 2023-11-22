using System.Runtime.Versioning;

namespace Plugin.FirebasePushNotifications.Platforms.Channels
{
    public class NotificationChannels : INotificationChannels
    {
        [SupportedOSPlatform("android26.0")]
        public void UpdateChannels()
        {
            StaticNotificationChannels.UpdateChannels();
        }
    }
}