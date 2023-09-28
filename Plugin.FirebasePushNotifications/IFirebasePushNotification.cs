namespace Plugin.FirebasePushNotifications
{
    public interface IFirebasePushNotification
    {
        /// <summary>
        /// Get all user notification categories
        /// </summary>
        NotificationUserCategory[] GetUserNotificationCategories();

        /// <summary>
        /// Get all subscribed topics
        /// </summary>
        string[] SubscribedTopics { get; }

        /// <summary>
        /// Subscribe to multiple topics
        /// </summary>
        void Subscribe(string[] topics);

        /// <summary>
        /// Subscribe to one topic
        /// </summary>
        void Subscribe(string topic);

        /// <summary>
        /// Unsubscribe to one topic
        /// </summary>
        void Unsubscribe(string topic);

        /// <summary>
        /// Unsubscribe to multiple topics
        /// </summary>
        void Unsubscribe(string[] topics);

        /// <summary>
        /// Unsubscribe all topics
        /// </summary>
        void UnsubscribeAll();

        /// <summary>
        /// Register push notifications on demand
        /// </summary>
        /// <returns></returns>
        void RegisterForPushNotifications();

        /// <summary>
        /// Unregister push notifications on demand
        /// </summary>
        /// <returns></returns>
        void UnregisterForPushNotifications();

        /// <summary>
        /// Notification handler to receive, customize notification feedback and provide user actions
        /// </summary>
        IPushNotificationHandler NotificationHandler { get; set; }

        /// <summary>
        /// Event triggered when token is refreshed
        /// </summary>
        event FirebasePushNotificationTokenEventHandler OnTokenRefresh;

        /// <summary>
        /// Event triggered when a notification is opened
        /// </summary>
        event FirebasePushNotificationResponseEventHandler OnNotificationOpened;

        /// <summary>
        /// Event triggered when a notification is opened by tapping an action
        /// </summary>
        event FirebasePushNotificationResponseEventHandler OnNotificationAction;

        /// <summary>
        /// Event triggered when a notification is received
        /// </summary>
        event FirebasePushNotificationDataEventHandler OnNotificationReceived;

        /// <summary>
        /// Event triggered when a notification is deleted
        /// </summary>
        event FirebasePushNotificationDataEventHandler OnNotificationDeleted;

        /// <summary>
        /// Event triggered when there's an error
        /// </summary>
        event FirebasePushNotificationErrorEventHandler OnNotificationError;

        /// <summary>
        /// Push notification token
        /// </summary>
        string Token { get; }

        Task<string> GetTokenAsync();

        /// <summary>
        /// Send device group message
        /// </summary>
        //void SendDeviceGroupMessage(IDictionary<string, string> parameters, string groupKey, string messageId, int timeOfLive);

        /// <summary>
        /// Clear all notifications
        /// </summary>
        void ClearAllNotifications();

        /// <summary>
        /// Remove specific id notification
        /// </summary>
        void RemoveNotification(int id);

        /// <summary>
        /// Remove specific id and tag notification
        /// </summary>
        void RemoveNotification(string tag, int id);
    }

    public delegate void FirebasePushNotificationTokenEventHandler(object source, FirebasePushNotificationTokenEventArgs e);

    public delegate void FirebasePushNotificationErrorEventHandler(object source, FirebasePushNotificationErrorEventArgs e);

    public delegate void FirebasePushNotificationDataEventHandler(object source, FirebasePushNotificationDataEventArgs e);

    public delegate void FirebasePushNotificationResponseEventHandler(object source, FirebasePushNotificationResponseEventArgs e);
}
