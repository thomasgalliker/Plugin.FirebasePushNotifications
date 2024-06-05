namespace Plugin.FirebasePushNotifications.Platforms
{
    /// <summary>
    /// T
    /// </summary>
    public interface INotificationBuilder
    {
        void OnNotificationReceived(IDictionary<string, object> data);
    }
}
