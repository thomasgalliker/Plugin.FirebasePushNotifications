namespace Plugin.FirebasePushNotifications
{
    public class FirebasePushNotificationResponseEventArgs : EventArgs
    {
        public string Identifier { get; }

        public IDictionary<string, object> Data { get; }

        public NotificationCategoryType Type { get; }

        public FirebasePushNotificationResponseEventArgs(IDictionary<string, object> data, string identifier = "", NotificationCategoryType type = NotificationCategoryType.Default)
        {
            this.Identifier = identifier;
            this.Data = data;
            this.Type = type;
        }

    }
}
