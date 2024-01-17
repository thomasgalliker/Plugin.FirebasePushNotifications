using Newtonsoft.Json;

namespace Plugin.FirebasePushNotifications
{
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

        [JsonProperty("categoryId")]
        public string CategoryId { get; }

        [JsonProperty("actions")]
        public NotificationAction[] Actions { get; }

        [JsonProperty("type")]
        public NotificationCategoryType Type { get; }

    }
}
