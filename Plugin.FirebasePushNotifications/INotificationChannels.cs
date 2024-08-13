namespace Plugin.FirebasePushNotifications
{
    /// <summary>
    /// Cross-platform interface to handle notification channels.
    /// </summary>
    /// <remarks>
    /// The concept of notification channels does only exist on Android.
    /// This cross-platform interfaces allows to update all existing
    /// notification channels from platform-independent code.
    /// </remarks>
    public partial interface INotificationChannels
    {
        /// <summary>
        /// Updates all existing notification channels.
        /// </summary>
        void UpdateChannels();
    }
}
