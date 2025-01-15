namespace Plugin.FirebasePushNotifications
{
    /// <summary>
    /// Cross-platform interface to handle notification channels.
    /// </summary>
    /// <remarks>
    /// The concept of notification channels exists on Android only.
    /// This interface is mainly used to simplify dependency injection.
    /// </remarks>
    public partial interface INotificationChannels
    {
    }
}