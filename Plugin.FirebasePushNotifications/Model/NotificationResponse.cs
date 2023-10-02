namespace Plugin.FirebasePushNotifications
{
    public class NotificationResponse
    {
        public string Identifier { get; }

        public IDictionary<string, object> Data { get; }

        public NotificationCategoryType Type { get; }

        public NotificationResponse(IDictionary<string, object> data, string identifier = "", NotificationCategoryType type = NotificationCategoryType.Default)
        {
            this.Identifier = identifier;
            this.Data = data;
            this.Type = type;
        }
    }
}
