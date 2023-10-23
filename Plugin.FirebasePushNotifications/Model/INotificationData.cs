namespace Plugin.FirebasePushNotifications.Model
{
    public interface INotificationData
    {
        public string Title { get; }
        
        public string Body { get; }

        public IDictionary<string, string> Data { get; }
    }
}