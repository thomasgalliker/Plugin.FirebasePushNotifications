using System.Diagnostics;
using Newtonsoft.Json;

namespace Plugin.FirebasePushNotifications
{
    /// <summary>
    /// A notification category consolidates a list of <see cref="NotificationAction"/>.
    /// Each notification category is identifiable via its <see cref="CategoryId"/>.
    /// </summary>
    [DebuggerDisplay("{CategoryId}")]
    public class NotificationCategory
    {
        public NotificationCategory(
            string categoryId,
            NotificationAction[] actions)
            : this(categoryId, actions, NotificationCategoryType.Default)
        {
        }

        [JsonConstructor]
        public NotificationCategory(
            [JsonProperty("categoryId")] string categoryId,
            [JsonProperty("actions")] NotificationAction[] actions,
            [JsonProperty("type")] NotificationCategoryType type)
        {
            this.CategoryId = categoryId ?? throw new ArgumentNullException(nameof(categoryId));

            if (actions == null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            if (actions.Length == 0)
            {
                throw new ArgumentException($"{nameof(actions)} must not be empty", nameof(actions));
            }

            this.Actions = actions;
            this.Type = type;
        }

        /// <summary>
        /// Identifier of the notification category.
        /// </summary>
        [JsonProperty("categoryId")]
        public string CategoryId { get; }

        /// <summary>
        /// Notification actions which belong to this notification category.
        /// </summary>
        [JsonProperty("actions")]
        public NotificationAction[] Actions { get; }

        /// <summary>
        /// Notification category type, used to display special-purpose
        /// notification categories. Default is <see cref="NotificationCategoryType.Default"/>.
        /// </summary>
        [JsonProperty("type")]
        public NotificationCategoryType Type { get; }

    }
}
