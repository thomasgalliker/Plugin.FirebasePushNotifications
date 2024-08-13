﻿namespace Plugin.FirebasePushNotifications
{
    internal static partial class Constants
    {
        internal const string NotificationTitleKey = "title";
        internal const string GcmNotificationTitleKey = "gcm.notification.title";
        internal const string NotificationBodyKey = "body";
        internal const string GcmNotificationBodyKey = "gcm.notification.body";
        internal const string NotificationTagKey = "tag";
        internal const string NotificationDataKey = "data";

        internal const string SuppressedString = "{suppressed}";

        internal static class Preferences
        {
            private const string KeyPrefix = "Plugin.FirebasePushNotifications";
            internal const string TokenKey = $"{KeyPrefix}.Token";
            internal const string SubscribedTopicsKey = $"{KeyPrefix}.SubscribedTopics";
            internal const string NotificationCategoriesKey = $"{KeyPrefix}.NotificationCategories";

            internal static readonly HashSet<string> AllKeys = new()
            {
                TokenKey,
                SubscribedTopicsKey,
                NotificationCategoriesKey,
            };
        }
    }
}
