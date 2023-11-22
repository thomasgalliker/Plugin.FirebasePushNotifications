using Plugin.FirebasePushNotifications.Internals;

namespace Plugin.FirebasePushNotifications
{
    public class FirebasePushNotificationTokenEventArgs : EventArgs
    {
        public string Token { get; }

        public FirebasePushNotificationTokenEventArgs(string token)
        {
            this.Token = token;
        }

        public override string ToString()
        {
            return $"Token={(this.Token is string token ? $"\"{TokenFormatter.AnonymizeToken(token)}\"" : "null")}";
        }
    }
}
