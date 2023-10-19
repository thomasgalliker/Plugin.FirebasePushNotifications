namespace Plugin.FirebasePushNotifications
{
    public class FirebasePushNotificationTokenEventArgs : EventArgs
    {
        public string Token { get; }

        public FirebasePushNotificationTokenEventArgs(string token)
        {
            this.Token = token;
        }
    }
}
