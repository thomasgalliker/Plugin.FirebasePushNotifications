namespace Plugin.FirebasePushNotifications.Platforms
{
    /// <summary>
    /// T
    /// </summary>
    public interface INotificationBuilder
    {
        /// <summary>
        /// Determines whether <see cref="OnNotificationReceived"/> should be called or not.
        /// </summary>
        /// <param name="data">The notification data.</param>
        bool ShouldHandleNotificationReceived(IDictionary<string, object> data);

        /// <summary>
        /// Displays a local notification popup with the given <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The notification data.</param>
        void OnNotificationReceived(IDictionary<string, object> data);
    }
}
