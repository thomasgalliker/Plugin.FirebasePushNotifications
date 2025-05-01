using System.Diagnostics.CodeAnalysis;
using Android.App;
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
        public static INotificationChannels Current => NotificationChannels.Current;

        /// <summary>
        /// Gets the list of configured notification channels.
        /// </summary>
        NotificationChannels.NotificationChannelsDelegate Channels { get; }

        /// <summary>
        /// Gets the list of configured notification channel groups.
        /// </summary>
        IEnumerable<NotificationChannelGroup> ChannelGroups { get; }

        /// <summary>
        /// Sets notification channel groups from given <paramref name="notificationChannelGroupRequests"/>.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="notificationChannelGroupRequests"/> already exist, they're updated.
        /// </remarks>
        /// <param name="notificationChannelGroupRequests">The notification channel group requests.</param>
        void SetNotificationChannelGroups(NotificationChannelGroupRequest[] notificationChannelGroupRequests);

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
        /// <param name="groupIds">The identifiers of the notification channel group.</param>
        void DeleteNotificationChannelGroups(string[] groupIds);

        /// <summary>
        /// Deletes all notification channel groups which are configured in <see cref="Channels"/>.
        /// </summary>
        void DeleteAllNotificationChannelGroups();

        /// <summary>
        /// Sets notification channels from given <paramref name="notificationChannelRequests"/>.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="notificationChannelRequests"/> already exist, they're updated.
        /// </remarks>
        /// <param name="notificationChannelRequests">The notification channel requests.</param>
        void SetNotificationChannels(NotificationChannelRequest[] notificationChannelRequests);

        /// <summary>
        /// Creates notification channels from given <paramref name="notificationChannelRequests"/>.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="notificationChannelRequests"/> already exist, they're updated.
        /// </remarks>
        /// <param name="notificationChannelRequests">The notification channel requests.</param>
        void CreateNotificationChannels([NotNull] NotificationChannelRequest[] notificationChannelRequests);

        /// <summary>
        /// Deletes the notification channel with the given <paramref name="notificationChannelId"/>.
        /// </summary>
        /// <param name="notificationChannelId">The notification channel identifier.</param>
        void DeleteNotificationChannel(string notificationChannelId);

        /// <summary>
        /// Deletes the notification channels with the given <paramref name="notificationChannelIds"/>.
        /// </summary>
        /// <param name="notificationChannelIds">The notification channel identifiers.</param>
        void DeleteNotificationChannels(string[] notificationChannelIds);

        /// <summary>
        /// Deletes all existing notification channels which are configured in <see cref="Channels"/>.
        /// </summary>
        void DeleteAllNotificationChannels();

        /// <summary>
        /// Open the notification settings.
        /// </summary>
        void OpenNotificationSettings();

        /// <summary>
        /// Opens the notification channel settings for <paramref name="notificationChannelId"/>.
        /// </summary>
        /// <param name="notificationChannelId">The notification channel identifier.</param>
        void OpenNotificationChannelSettings(string notificationChannelId);
    }
}