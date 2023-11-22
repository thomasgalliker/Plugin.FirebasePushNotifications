namespace Plugin.FirebasePushNotifications
{
    public class NotificationAction
    {
        public NotificationAction(
            string id, 
            string title, 
            NotificationActionType notificationActionType = NotificationActionType.Default, 
            string icon = null)
        {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
            this.Title = title ?? throw new ArgumentNullException(nameof(title));
            this.Type = notificationActionType;
            this.Icon = icon;
        }
        
        public string Id { get; }

        public string Title { get; }
        
        public NotificationActionType Type { get; }
        
        public string Icon { get; }

        public override string ToString()
        {
            return this.Id;
        }
    }
}
