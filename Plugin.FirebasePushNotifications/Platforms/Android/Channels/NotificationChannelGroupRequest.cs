using System.Diagnostics;

namespace Plugin.FirebasePushNotifications.Platforms.Channels
{
    /// <summary>
    /// Notification channel group request.
    /// </summary>
    [DebuggerDisplay("{GroupId}")]
    public class NotificationChannelGroupRequest
    {
        /// <summary>
        /// Creates a new instance of <see cref="NotificationChannelRequest"/>.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="name">The group name.</param>
        /// <param name="description">The group description (optional).</param>
        public NotificationChannelGroupRequest(string groupId, string name, string description = null)
        {
            if (string.IsNullOrEmpty(groupId))
            {
                throw new ArgumentException("The group identifier must not be null or empty", nameof(groupId));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The group name must not be null or empty", nameof(name));
            }

            this.GroupId = groupId;
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// The group identifier.
        /// </summary>
        public string GroupId { get; }

        /// <summary>
        /// The group name (displayed in the notification settings of the app).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The group description (optional).
        /// </summary>
        public string Description { get; }
    }
}