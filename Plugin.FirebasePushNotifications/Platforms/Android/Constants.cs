using Plugin.FirebasePushNotifications.Platforms.Channels;

namespace Plugin.FirebasePushNotifications
{
    internal static partial class Constants
    {
        internal const string ExtraFirebaseProcessIntentHandled = "EXTRA_FIREBASE_PROCESS_INTENT_HANDLED";

        public const string DefaultNotificationChannelId = "DefaultNotificationChannel";
        public const string DefaultNotificationChannelName = "Default";

        internal static readonly NotificationChannelRequest DefaultNotificationChannel = new NotificationChannelRequest
        {
            ChannelId = Constants.DefaultNotificationChannelId,
            ChannelName = Constants.DefaultNotificationChannelName,
            IsDefault = true,
        };

        public const string MetadataIconKey = "com.google.firebase.messaging.default_notification_icon";
        public const string MetadataColorKey = "com.google.firebase.messaging.default_notification_color";

        public const string CategoryKey = "category";
        public const string TextKey = "text";
        public const string SubtitleKey = "subtitle";
        public const string MessageKey = "message";
        public const string AlertKey = "alert";
        public const string IdKey = "id";
        public const string ClickActionKey = "click_action";
        public const string NotificationCategoryKey = "notification_category";
        public const string UseFullIntentKey = "use_full_intent";
        public const string OnGoingKey = "ongoing";
        public const string SilentKey = "silent";
        public const string ActionNotificationIdKey = "action_notification_id";
        public const string ActionNotificationTagKey = "action_notification_tag";
        public const string NotificationActionId = "notification_action_id";
        public const string ColorKey = "color";
        public const string IconKey = "icon";
        public const string LargeIconKey = "large_icon";
        public const string SoundKey = "sound";
        public const string PriorityKey = "priority";
        public const string ChannelIdKey = "channel_id";
        public const string ShowWhenKey = "show_when";
        public const string BigTextStyleKey = "bigtextstyle";
    }
}
