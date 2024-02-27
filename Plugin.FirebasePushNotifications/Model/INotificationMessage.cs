namespace Plugin.FirebasePushNotifications.Model
{
    public interface INotification
    {
    }
    
    public interface INotificationAction
    {
        public string ActionIdentifier { get; set; }
    }

    public interface INotificationMessage : INotification
    {
        public string Title { get; }
        
        public string Body { get; }

        public IDictionary<string, string> Data { get; }
    }
}