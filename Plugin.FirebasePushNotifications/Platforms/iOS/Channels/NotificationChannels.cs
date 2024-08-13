namespace Plugin.FirebasePushNotifications.Platforms.Channels
{
    /// <summary>
    /// NotificationChannels implementation for iOS.
    /// This implementation is just created to allow cross-platform recreation of notification channels on Android.
    /// </summary>
    public class NotificationChannels : INotificationChannels
    {
        /// <inheritdoc />
        public void UpdateChannels()
        {
            // Do nothing on iOS...
        }
    }
}
