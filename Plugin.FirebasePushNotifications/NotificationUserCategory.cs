using Plugin.FirebasePushNotifications;

namespace Plugin.FirebasePushNotifications
{
    public class NotificationUserCategory
    {
        public string Category { get; }

        public List<NotificationUserAction> Actions { get; }

        public NotificationCategoryType Type { get; }

        public NotificationUserCategory(string category, List<NotificationUserAction> actions, NotificationCategoryType type = NotificationCategoryType.Default)
        {
            this.Category = category;
            this.Actions = actions;
            this.Type = type;
        }
    }
}
