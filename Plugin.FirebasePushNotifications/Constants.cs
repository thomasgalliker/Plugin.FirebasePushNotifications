namespace Plugin.FirebasePushNotifications
{
    internal static partial class Constants
    {
        internal const string NotificationTitleKey = "title";
        internal const string NotificationBodyKey = "body";
        internal const string NotificationTagKey = "tag";
        internal const string NotificationDataKey = "data";

        internal const string SuppressedString = "{suppressed}";

        internal static class Preferences
        {
            internal const string SharedName = "Plugin.FirebasePushNotifications";
            internal static readonly string KeyPrefix = "Plugin.FirebasePushNotifications";

            internal static readonly string TokenKey = $"{KeyPrefix}.Token";
            internal static readonly string SubscribedTopicsKey = $"{KeyPrefix}.SubscribedTopics";
            internal static readonly string NotificationCategoriesKey = $"{KeyPrefix}.NotificationCategories";

            internal static readonly HashSet<string> AllKeys =
            [
                TokenKey,
                SubscribedTopicsKey,
                NotificationCategoriesKey,
            ];
        }
    }
}
