using Plugin.FirebasePushNotifications.Platforms.Channels;

namespace Plugin.FirebasePushNotifications
{
    /// <summary>
    /// Android-specific interface to handle notification channels.
    /// </summary>
    public partial interface INotificationChannels
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="INotificationChannels"/>.
        /// </summary>
        public static INotificationChannels Current { get; set; } = NotificationChannels.Current;

        /// <summary>
        /// Gets the list of configured notification channels.
        /// </summary>
        IEnumerable<NotificationChannelRequest> Channels { get; }

        /// <summary>
        /// Creates a notification channel group.
        /// </summary>
        /// <remarks>
        /// Important: Create notification channel groups before you create notification channels!
        /// </remarks>
        /// <param name="notificationChannelGroupRequest">The notification channel group request.</param>
        void CreateNotificationChannelGroup(NotificationChannelGroupRequest notificationChannelGroupRequest);

        /// <summary>
        /// Creates notification channel groups.
        /// </summary>
        /// <remarks>
        /// Important: Create notification channel groups before you create notification channels!
        /// </remarks>
        /// <param name="notificationChannelGroupRequests">The notification channel group requests.</param>
        void CreateNotificationChannelGroups(NotificationChannelGroupRequest[] notificationChannelGroupRequests);

        /// <summary>
        /// Deletes the notification channel group with <paramref name="groupId"/>.
        /// </summary>
        /// <param name="groupId">The identifier of the notification channel group.</param>
        void DeleteNotificationChannelGroup(string groupId);

        /// <summary>
        /// Deletes the notification channel groups with <paramref name="groupIds"/>.
        /// </summary>
        /// <param name="groupIds">The identifiers of the notification channel groups.</param>
        void DeleteNotificationChannelGroups(string[] groupIds);

        /// <summary>
        /// Deletes all notification channel groups which are configured in <see cref="Channels"/>.
        /// </summary>
        void DeleteAllNotificationChannelGroups();

        /// <summary>
        /// Creates notification channels from given <paramref name="notificationChannelRequests"/>.
        /// </summary>
        /// <param name="notificationChannelRequests">The notification channel requests.</param>
        void CreateChannels(NotificationChannelRequest[] notificationChannelRequests);

        /// <summary>
        /// Updates notification channels from given <paramref name="notificationChannelRequests"/>.
        /// </summary>
        /// <param name="notificationChannelRequests">The notification channel requests.</param>
        void UpdateChannels(NotificationChannelRequest[] notificationChannelRequests);

        /// <summary>
        /// Deletes notification channels with identifiers <paramref name="channelIds"/>.
        /// </summary>
        /// <param name="channelIds">The notification channel requests.</param>
        void DeleteChannels(string[] channelIds);

        /// <summary>
        /// Deletes all existing notification channels which are configured in <see cref="Channels"/>.
        /// </summary>
        void DeleteAllChannels();
    }
}