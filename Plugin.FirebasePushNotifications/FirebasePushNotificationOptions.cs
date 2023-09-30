namespace Plugin.FirebasePushNotifications
{
    public sealed class FirebasePushNotificationOptions
    {
        public bool AutoInit { get; init; }

        public override string ToString()
        {
            return $"[{nameof(FirebasePushNotificationOptions)}: " +
                   $"{nameof(this.AutoInit)}={this.AutoInit},"
                   ;
        }
    }
}