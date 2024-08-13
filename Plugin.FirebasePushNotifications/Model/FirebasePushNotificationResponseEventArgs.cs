using Plugin.FirebasePushNotifications.Extensions;

namespace Plugin.FirebasePushNotifications
{
    public class FirebasePushNotificationResponseEventArgs : FirebasePushNotificationDataEventArgs
    {
        public FirebasePushNotificationResponseEventArgs(IDictionary<string, object> data, NotificationCategoryType notificationCategoryType)
            : base(data)
        {
            this.Type = notificationCategoryType;
        }

        public NotificationCategoryType Type { get; }

        public override string ToString()
        {
            return $"Type={this.Type}, Data=[{this.Data.ToDebugString()}]";
        }
    }
}
