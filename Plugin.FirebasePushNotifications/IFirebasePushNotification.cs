#if ANDROID
using Android.App;
using Android.Content;
#endif

#if IOS
using Foundation;
using UIKit;
#endif

using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Platforms;

namespace Plugin.FirebasePushNotifications
{
    public interface IFirebasePushNotification
    {
        /// <summary>
        /// Configures this instance of <see cref="IFirebasePushNotification"/>
        /// with <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The firebase push notification options.</param>
        void Configure(FirebasePushNotificationOptions options);

        /// <summary>
        /// Clears all queues (if any exist).
        /// </summary>
        /// <remarks>
        /// This is usually done when the content in the queues is no longer needed,
        /// e.g. when the user is logged-out or if the queued data has become outdated.
        /// </remarks>
        void ClearQueues();

        /// <summary>
        /// Sets the logger instance.
        /// </summary>
        /// <remarks>
        /// The logger instance can be injected at runtime.
        /// This is helpful since <see cref="CrossFirebasePushNotification.Current"/> is a singleton instance 
        /// and does therefore not allow to inject any logger via constructor injection.
        /// </remarks>
        ILogger<FirebasePushNotificationManager> Logger { set; }

        void HandleNotificationReceived(IDictionary<string, object> data);

        void HandleNotificationAction(IDictionary<string, object> data, string identifier, NotificationCategoryType notificationCategoryType);

        void HandleNotificationDeleted(IDictionary<string, object> data);

        void HandleTokenRefresh(string token);

#if ANDROID
        /// <summary>
        /// ProcessIntent is called OnCreate and OnNewIntent in order to check
        /// for incoming push/local notifications.
        /// This method is automatically called when you add
        /// <see cref="MauiAppBuilderExtensions.UseFirebasePushNotifications"/>
        /// to your MauiProgram startup.
        /// </summary>
        void ProcessIntent(Activity activity, Intent intent);
#endif

#if IOS
        void RegisteredForRemoteNotifications(NSData deviceToken);

        void FailedToRegisterForRemoteNotifications(NSError error);

        void DidReceiveRemoteNotification(NSDictionary userInfo);

        void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler);
#endif

        /// <summary>
        /// Get all user notification categories.
        /// </summary>
        NotificationUserCategory[] GetUserNotificationCategories();

        /// <summary>
        /// Get all subscribed topics.
        /// </summary>
        string[] SubscribedTopics { get; }

        /// <summary>
        /// Subscribe to multiple topics.
        /// </summary>
        void Subscribe(string[] topics);

        /// <summary>
        /// Subscribe to one topic.
        /// </summary>
        void Subscribe(string topic);

        /// <summary>
        /// Unsubscribe from one topic.
        /// </summary>
        void Unsubscribe(string topic);

        /// <summary>
        /// Unsubscribe to multiple topics.
        /// </summary>
        void Unsubscribe(string[] topics);

        /// <summary>
        /// Unsubscribe all topics.
        /// </summary>
        void UnsubscribeAll();

        /// <summary>
        /// Register for push notifications.
        /// </summary>
        Task RegisterForPushNotificationsAsync();

        /// <summary>
        /// Unregister push notifications.
        /// </summary>
        void UnregisterForPushNotifications();

        /// <summary>
        /// Notification handler to receive, customize notification feedback and provide user actions
        /// </summary>
        IPushNotificationHandler NotificationHandler { get; set; }

        /// <summary>
        /// Event triggered when token is refreshed.
        /// </summary>
        event EventHandler<FirebasePushNotificationTokenEventArgs> TokenRefreshed;

        /// <summary>
        /// Event triggered when a notification is opened.
        /// </summary>
        event EventHandler<FirebasePushNotificationResponseEventArgs> NotificationOpened;

        /// <summary>
        /// Event triggered when a notification is opened by tapping an action.
        /// </summary>
        event EventHandler<FirebasePushNotificationResponseEventArgs> NotificationAction;

        /// <summary>
        /// Event triggered when a notification is received.
        /// </summary>
        event EventHandler<FirebasePushNotificationDataEventArgs> NotificationReceived;

        /// <summary>
        /// Event triggered when a notification is deleted.
        /// </summary>
        event EventHandler<FirebasePushNotificationDataEventArgs> NotificationDeleted;

        /// <summary>
        /// Event triggered when an error has occurred.
        /// </summary>
        event EventHandler<FirebasePushNotificationErrorEventArgs> NotificationError;

        /// <summary>
        /// The push notification token.
        /// </summary>
        string Token { get; }

        /// <summary>
        /// Send device group message
        /// </summary>
        //void SendDeviceGroupMessage(IDictionary<string, string> parameters, string groupKey, string messageId, int timeOfLive);

        /// <summary>
        /// Clear all notifications.
        /// </summary>
        void ClearAllNotifications();

        /// <summary>
        /// Remove specific id notification.
        /// </summary>
        void RemoveNotification(int id);

        /// <summary>
        /// Remove specific id and tag notification.
        /// </summary>
        void RemoveNotification(string tag, int id);
    }
}
