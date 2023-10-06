namespace Plugin.FirebasePushNotifications
{
    public class FirebasePushNotificationErrorEventArgs : EventArgs
    {
        public FirebasePushNotificationErrorType Type;
        public string Message { get; }

        public FirebasePushNotificationErrorEventArgs(FirebasePushNotificationErrorType type, string message)
        {
            this.Type = type;
            this.Message = message;
        }

    }
}
