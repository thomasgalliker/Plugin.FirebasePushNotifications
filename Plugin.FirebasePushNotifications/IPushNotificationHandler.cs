namespace Plugin.FirebasePushNotifications
{
    public interface IPushNotificationHandler
    {
        /// <summary>
        /// Method triggered when an error occurs.
        /// </summary>
        /// <param name="error">The error message.</param>
        void OnError(string error);

        /// <summary>
        /// Method triggered when a notification is opened
        /// </summary>
        /// <param name="response">The notification response.</param>
        void OnOpened(NotificationResponse response);

        /// <summary>
        /// Method triggered when a notification is received.
        /// </summary>
        /// <param name="parameters">The notification data.</param>
        void OnReceived(IDictionary<string, object> parameters);
    }
}