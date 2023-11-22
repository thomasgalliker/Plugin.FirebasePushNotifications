namespace Plugin.FirebasePushNotifications
{
    public class FirebasePushNotificationErrorEventArgs : EventArgs
    {
        public FirebasePushNotificationErrorEventArgs(FirebasePushNotificationErrorType type, string message)
        {
            this.Type = type;
            this.Message = message;
        }

        public FirebasePushNotificationErrorType Type;

        public string Message { get; }

        public override string ToString()
        {
            return $"Type={this.Type}, Message={(this.Message is string message ? $"\"{message}\"" : "null")}";
        }
    }
}
