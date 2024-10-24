using Plugin.FirebasePushNotifications.Platforms.Channels;

namespace Plugin.FirebasePushNotifications
{
    /// <summary>
    /// Android-specific interface to handle notification channels.
    /// </summary>
    public partial interface INotificationChannels
    {
        /// <summary>
        /// Gets the list of configured notification channels.
        /// </summary>
        IEnumerable<NotificationChannelRequest> Channels { get; }

        /// <summary>
        /// Create Android notification channels from given <paramref name="notificationChannelRequests"/>.
        /// </summary>
        /// <param name="notificationChannelRequests">The notification channel requests.</param>
        void CreateChannels(NotificationChannelRequest[] notificationChannelRequests);

        /// <summary>
        /// Update Android notification channels from given <paramref name="notificationChannelRequests"/>.
        /// </summary>
        /// <param name="notificationChannelRequests">The notification channel requests.</param>
        void UpdateChannels(NotificationChannelRequest[] notificationChannelRequests);

        /// <summary>
        /// Delete Android notification channels from given <paramref name="channelIds"/>.
        /// </summary>
        /// <param name="channelIds">The notification channel identifiers.</param>
        void DeleteChannels(string[] channelIds);

        /// <summary>
        /// Deletes all existing notification channels.
        /// </summary>
        void DeleteAllChannels();
    }
}