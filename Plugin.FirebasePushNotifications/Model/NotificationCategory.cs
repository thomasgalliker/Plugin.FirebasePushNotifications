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

        public NotificationCategory(
            string categoryId,
            NotificationAction[] actions,
            NotificationCategoryType type)
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

        public string CategoryId { get; }

        public NotificationAction[] Actions { get; } = Array.Empty<NotificationAction>();

        public NotificationCategoryType Type { get; }

    }
}
