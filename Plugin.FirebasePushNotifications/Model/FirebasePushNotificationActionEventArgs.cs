using Plugin.FirebasePushNotifications.Extensions;

namespace Plugin.FirebasePushNotifications
{
    public class FirebasePushNotificationActionEventArgs : FirebasePushNotificationDataEventArgs
    {
        public FirebasePushNotificationActionEventArgs(IDictionary<string, object> data, NotificationAction notificationAction, NotificationCategoryType notificationCategoryType)
            : base(data)
        {
            this.Action = notificationAction;
            this.Type = notificationCategoryType;
        }

        public NotificationAction Action { get; }

        public NotificationCategoryType Type { get; }

        public override string ToString()
        {
            return $"Action.Id={(this.Action is NotificationAction action ? $"\"{action.Id}\"" : "null")}, Type={this.Type}, Data=[{this.Data.ToDebugString()}]";
        }
    }
}