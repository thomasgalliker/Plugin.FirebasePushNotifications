namespace Plugin.FirebasePushNotifications
{
    public interface IPushNotificationHandler
    {
        /// <summary>
        /// Method triggered when a notification is opened
        /// </summary>
        void OnOpened(IDictionary<string, object> parameters, NotificationCategoryType notificationCategoryType);

        /// <summary>
        /// Method triggered when a notification is received.
        /// </summary>
        /// <param name="parameters">The notification data.</param>
        void OnReceived(IDictionary<string, object> parameters);

        //void OnDeleted(IDictionary<string, object> parameters);
    }
}