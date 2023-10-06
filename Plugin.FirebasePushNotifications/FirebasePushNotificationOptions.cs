namespace Plugin.FirebasePushNotifications
{
    public class FirebasePushNotificationOptions
    {
        public virtual bool AutoInit { get; set; }

        public override string ToString()
        {
            return $"[{nameof(FirebasePushNotificationOptions)}: " +
                   $"{nameof(this.AutoInit)}={this.AutoInit},"
                   ;
        }
    }
}