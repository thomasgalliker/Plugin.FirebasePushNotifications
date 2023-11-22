using Plugin.FirebasePushNotifications.Extensions;

namespace Plugin.FirebasePushNotifications
{
    public class FirebasePushNotificationDataEventArgs : EventArgs
    {
        public IDictionary<string, object> Data { get; }

        public FirebasePushNotificationDataEventArgs(IDictionary<string, object> data)
        {
            this.Data = data;
        }

        public override string ToString()
        {
            return $"Data=[{this.Data.ToDebugString()}]";
        }
    }
}
