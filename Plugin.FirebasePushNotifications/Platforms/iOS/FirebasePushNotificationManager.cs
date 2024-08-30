using Foundation;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Extensions;
using UIKit;
using UserNotifications;

namespace Plugin.FirebasePushNotifications.Platforms
{
    /// <summary>
    /// Implementation of <see cref="IFirebasePushNotification"/>
    /// for iOS.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class FirebasePushNotificationManager : FirebasePushNotificationManagerBase, IFirebasePushNotification
    {
        private const UNAuthorizationOptions AuthorizationOptions =
            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;

        private readonly Queue<(string Topic, bool Subscribe)> pendingTopics = new Queue<(string, bool)>();
        private bool disposed;

        internal FirebasePushNotificationManager(
            ILogger<FirebasePushNotificationManager> logger,
            ILoggerFactory loggerFactory,
            FirebasePushNotificationOptions options,
            IPushNotificationHandler pushNotificationHandler,
            IFirebasePushNotificationPreferences preferences)
           : base(logger, loggerFactory, options, pushNotificationHandler, preferences)
        {
            this.ConfigurePlatform(options);
        }

        /// <inheritdoc />
        public string Token
        {
            get
            {
                var fcmToken = Firebase.CloudMessaging.Messaging.SharedInstance.FcmToken;
                if (!string.IsNullOrEmpty(fcmToken))
                {
                    return fcmToken;
                }
                else
                {
                    return this.preferences.Get<string>(Constants.Preferences.TokenKey);
                }
            }
        }

        /// <inheritdoc />
        protected override void RegisterNotificationCategoriesPlatform(NotificationCategory[] notificationCategories)
        {
            var unNotificationCategories = new List<UNNotificationCategory>();

            foreach (var notificationCategory in notificationCategories)
            {
                var notificationActions = new List<UNNotificationAction>();

                foreach (var action in notificationCategory.Actions)
                {
                    var notificationActionType = GetUNNotificationActionOptions(action.Type);
                    var notificationAction = UNNotificationAction.FromIdentifier(action.Id, action.Title, notificationActionType);
                    notificationActions.Add(notificationAction);
                }

                // Create UNNotificationCategory
                var options = notificationCategory.Type == NotificationCategoryType.Dismiss
                    ? UNNotificationCategoryOptions.CustomDismissAction
                    : UNNotificationCategoryOptions.None;

                var unNotificationCategory = UNNotificationCategory.FromIdentifier(
                    identifier: notificationCategory.CategoryId,
                    actions: notificationActions.ToArray(),
                    intentIdentifiers: Array.Empty<string>(),
                    options);
                unNotificationCategories.Add(unNotificationCategory);
            }

            // Register categories
            var notificationCategoriesSet = new NSSet<UNNotificationCategory>(unNotificationCategories.ToArray());
            UNUserNotificationCenter.Current.SetNotificationCategories(notificationCategoriesSet);
        }

        /// <inheritdoc />
        protected override void ClearNotificationCategoriesPlatform()
        {
            var categories = new NSSet<UNNotificationCategory>(Array.Empty<UNNotificationCategory>());
            UNUserNotificationCenter.Current.SetNotificationCategories(categories);
        }

        private static UNNotificationActionOptions GetUNNotificationActionOptions(NotificationActionType type)
        {
            UNNotificationActionOptions notificationActionType;

            switch (type)
            {
                case NotificationActionType.AuthenticationRequired:
                    notificationActionType = UNNotificationActionOptions.AuthenticationRequired;
                    break;
                case NotificationActionType.Destructive:
                    notificationActionType = UNNotificationActionOptions.Destructive;
                    break;
                case NotificationActionType.Foreground:
                    notificationActionType = UNNotificationActionOptions.Foreground;
                    break;
                default:
                    notificationActionType = UNNotificationActionOptions.None;
                    break;
            }

            return notificationActionType;
        }

        private void ConfigurePlatform(FirebasePushNotificationOptions options)
        {
            var firebaseMessaging = Firebase.CloudMessaging.Messaging.SharedInstance;

            if (firebaseMessaging == null)
            {
                var sharedInstanceNullErrorMessage = "Firebase.CloudMessaging.Messaging.SharedInstance is null";
                this.logger.LogError(sharedInstanceNullErrorMessage);
                throw new NullReferenceException(sharedInstanceNullErrorMessage);
            }

            if (UNUserNotificationCenter.Current.Delegate != null)
            {
                this.logger.LogWarning("UNUserNotificationCenter.Current.Delegate is already set");
            }
            else
            {
                UNUserNotificationCenter.Current.Delegate = new UNUserNotificationCenterDelegateImpl(
                    this.DidReceiveNotificationResponse,
                    this.WillPresentNotification);
            }

            if (firebaseMessaging.Delegate != null)
            {
                this.logger.LogWarning("Firebase.CloudMessaging.Messaging.SharedInstance.Delegate is already set");
            }
            else
            {
                firebaseMessaging.Delegate = new MessagingDelegateImpl((_, fcmToken) => this.DidReceiveRegistrationToken(fcmToken));
            }
        }

        /// <inheritdoc />
        public async Task RegisterForPushNotificationsAsync()
        {
            this.logger.LogDebug("RegisterForPushNotificationsAsync");

            Firebase.CloudMessaging.Messaging.SharedInstance.AutoInitEnabled = true;

            var (granted, error) = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(AuthorizationOptions);
            if (granted)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    UIApplication.SharedApplication.RegisterForRemoteNotifications();
                });
            }
            else
            {
                this.logger.LogWarning("RegisterForPushNotificationsAsync: Push notification permission denied by user");
            }

            if (error != null)
            {
                var exception = new Exception("RegisterForPushNotificationsAsync failed with exception", new NSErrorException(error));
                this.logger.LogError(exception, exception.Message);
                throw exception;
            }
        }

        private static bool HasApnsToken => Firebase.CloudMessaging.Messaging.SharedInstance.ApnsToken != null;

        /// <inheritdoc />
        public async Task UnregisterForPushNotificationsAsync()
        {
            this.logger.LogDebug("UnregisterForPushNotificationsAsync");

            Firebase.CloudMessaging.Messaging.SharedInstance.AutoInitEnabled = false;

            // if (Firebase.CloudMessaging.Messaging.SharedInstance.Delegate is MessagingDelegateImpl)
            // {
            //     Firebase.CloudMessaging.Messaging.SharedInstance.Delegate = null;
            // }

            await MainThread.InvokeOnMainThreadAsync(UIApplication.SharedApplication.UnregisterForRemoteNotifications);

            Firebase.CloudMessaging.Messaging.SharedInstance.ApnsToken = null;

            this.preferences.Remove(Constants.Preferences.TokenKey);
        }

        /// <inheritdoc />
        public void RegisteredForRemoteNotifications(NSData deviceToken)
        {
            this.logger.LogDebug("RegisteredForRemoteNotifications");

            Firebase.CloudMessaging.Messaging.SharedInstance.ApnsToken = deviceToken;

            this.DidReceiveRegistrationToken(Firebase.CloudMessaging.Messaging.SharedInstance.FcmToken);
        }

        /// <inheritdoc />
        public void FailedToRegisterForRemoteNotifications(NSError error)
        {
            this.logger.LogError(new NSErrorException(error), "FailedToRegisterForRemoteNotifications");
        }

        /// <inheritdoc />
        public void DidReceiveRemoteNotification(NSDictionary userInfo)
        {
            this.logger.LogDebug("DidReceiveRemoteNotification");

            this.DidReceiveRemoteNotificationInternal(userInfo);
        }

        /// <inheritdoc />
        public void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            this.logger.LogDebug("DidReceiveRemoteNotification(UIApplication, NSDictionary, Action<UIBackgroundFetchResult>)");

            // If you are receiving a notification message while your app is in the background,
            // this callback will not be fired 'till the user taps on the notification launching the application.

            // If you disable method swizzling, you'll need to call this method.
            // This lets FCM track message delivery and analytics, which is performed
            // automatically with method swizzling enabled.
            this.DidReceiveRemoteNotificationInternal(userInfo);

            completionHandler(UIBackgroundFetchResult.NewData);
        }

        private void DidReceiveRemoteNotificationInternal(NSDictionary userInfo)
        {
            Firebase.CloudMessaging.Messaging.SharedInstance.AppDidReceiveMessage(userInfo);
            var data = userInfo.GetParameters();
            this.HandleNotificationReceived(data);
        }

        private void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            var data = notification.Request.Content.UserInfo.GetParameters();
            var notificationPresentationOptions = GetNotificationPresentationOptions(data);
            this.logger.LogDebug($"WillPresentNotification: UNNotificationPresentationOptions={notificationPresentationOptions}");

            this.HandleNotificationReceived(data);

            completionHandler(notificationPresentationOptions);
        }

        private static UNNotificationPresentationOptions GetNotificationPresentationOptions(IDictionary<string, object> data)
        {
            var notificationPresentationOptions = UNNotificationPresentationOptions.None;

            if (data.TryGetValue("priority", out var p) && $"{p}".ToLower() is string priority)
            {
                if (priority is "high" or "max")
                {
                    if (UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
                    {
                        if (!notificationPresentationOptions.HasFlag(UNNotificationPresentationOptions.List | UNNotificationPresentationOptions.Banner))
                        {
                            notificationPresentationOptions |= UNNotificationPresentationOptions.List | UNNotificationPresentationOptions.Banner;
                        }
                    }
                    else
                    {
                        if (!notificationPresentationOptions.HasFlag(UNNotificationPresentationOptions.Alert))
                        {
                            notificationPresentationOptions |= UNNotificationPresentationOptions.Alert;
                        }
                    }
                }
                else if (priority is "default" or "low" or "min")
                {
                    if (UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
                    {
                        if (!notificationPresentationOptions.HasFlag(UNNotificationPresentationOptions.List | UNNotificationPresentationOptions.Banner))
                        {
                            notificationPresentationOptions &= UNNotificationPresentationOptions.List | UNNotificationPresentationOptions.Banner;
                        }
                    }
                    else
                    {
                        if (!notificationPresentationOptions.HasFlag(UNNotificationPresentationOptions.Alert))
                        {
                            notificationPresentationOptions &= UNNotificationPresentationOptions.Alert;
                        }
                    }
                }
            }

            return notificationPresentationOptions;
        }

        /// <inheritdoc />
        public void SubscribeTopics(string[] topics)
        {
            foreach (var t in topics)
            {
                this.SubscribeTopic(t);
            }
        }

        /// <inheritdoc />
        public void SubscribeTopic(string topic)
        {
            if (topic == null)
            {
                throw new ArgumentNullException(nameof(topic), "Topic must not be null");
            }

            if (topic == string.Empty)
            {
                throw new ArgumentException("Topic must not be empty", nameof(topic));
            }

            if (!HasApnsToken)
            {
                this.pendingTopics.Enqueue((topic, true));
                return;
            }

            var subscribedTopics = new HashSet<string>(this.SubscribedTopics);
            if (!subscribedTopics.Contains(topic))
            {
                this.logger.LogDebug($"Subscribe: topic=\"{topic}\"");

                Firebase.CloudMessaging.Messaging.SharedInstance.Subscribe(topic);
                subscribedTopics.Add(topic);

                this.SubscribedTopics = subscribedTopics.ToArray();
            }
            else
            {
                this.logger.LogInformation($"Subscribe: skipping topic \"{topic}\"; topic is already subscribed");
            }
        }

        /// <inheritdoc />
        public void UnsubscribeAllTopics()
        {
            var topics = this.SubscribedTopics.ToArray();
            this.logger.LogDebug($"UnsubscribeAllTopics: topics=[{string.Join(',', topics)}]");

            foreach (var topic in topics)
            {
                this.logger.LogDebug($"Unsubscribe: topic=\"{topic}\"");
                Firebase.CloudMessaging.Messaging.SharedInstance.Unsubscribe(topic);
            }

            this.SubscribedTopics = null;
        }

        /// <inheritdoc />
        public void UnsubscribeTopics(string[] topics)
        {
            if (topics == null)
            {
                throw new ArgumentNullException(nameof(topics), $"Parameter '{nameof(topics)}' must not be null");
            }

            // TODO: Improve efficiency here (move to base class maybe)
            foreach (var t in topics)
            {
                this.UnsubscribeTopic(t);
            }
        }

        /// <inheritdoc />
        public void UnsubscribeTopic(string topic)
        {
            if (topic == null)
            {
                throw new ArgumentNullException(nameof(topic), "Topic must not be null");
            }

            if (topic == string.Empty)
            {
                throw new ArgumentException("Topic must not be empty", nameof(topic));
            }

            if (!HasApnsToken)
            {
                this.pendingTopics.Enqueue((topic, false));
                return;
            }

            var subscribedTopics = new HashSet<string>(this.SubscribedTopics);
            if (subscribedTopics.Contains(topic))
            {
                this.logger.LogDebug($"Unsubscribe: topic=\"{topic}\"");

                Firebase.CloudMessaging.Messaging.SharedInstance.Unsubscribe(topic);
                subscribedTopics.Remove(topic);

                this.SubscribedTopics = subscribedTopics.ToArray();
            }
            else
            {
                this.logger.LogInformation($"Unsubscribe: skipping topic \"{topic}\"; topic is not subscribed");
            }
        }

        private void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            this.logger.LogDebug("DidReceiveNotificationResponse");

            var data = response.Notification.Request.Content.UserInfo.GetParameters();

            NotificationCategoryType notificationCategoryType;

            if (response.IsCustomAction)
            {
                notificationCategoryType = NotificationCategoryType.Custom;
            }
            else if (response.IsDismissAction)
            {
                notificationCategoryType = NotificationCategoryType.Dismiss;
            }
            else
            {
                notificationCategoryType = NotificationCategoryType.Default;
            }
            const string defaultActionIdentifier = "com.apple.UNNotificationDefaultActionIdentifier";
            var actionIdentifier = string.Equals(response.ActionIdentifier, defaultActionIdentifier, StringComparison.InvariantCultureIgnoreCase)
                ? null
                : response.ActionIdentifier;

            if (string.IsNullOrEmpty(actionIdentifier))
            {
                this.HandleNotificationOpened(data, notificationCategoryType);
            }
            else
            {
                var categoryIdentifier = response.Notification.Request.Content.CategoryIdentifier;
                this.HandleNotificationAction(data, categoryIdentifier, actionIdentifier, notificationCategoryType);
            }

            // Inform caller it has been handled
            completionHandler();
        }

        private void DidReceiveRegistrationToken(string fcmToken)
        {
            this.logger.LogDebug("DidReceiveRegistrationToken");

            // Note that this callback will be fired everytime a new token is generated,
            // including the first time a token is received.

            if (string.IsNullOrEmpty(fcmToken))
            {
                return;
            }

            this.HandleTokenRefresh(fcmToken);

            this.TryDequeuePendingTopics();
        }

        private void TryDequeuePendingTopics()
        {
            if (!HasApnsToken)
            {
                return;
            }

            while (this.pendingTopics.TryDequeue(out var pendingTopic))
            {
                if (pendingTopic.Subscribe)
                {
                    this.SubscribeTopic(pendingTopic.Topic);
                }
                else
                {
                    this.UnsubscribeTopic(pendingTopic.Topic);
                }
            }
        }

        /// <inheritdoc />
        protected override void ClearAllNotificationsPlatform()
        {
            // Remove all delivered notifications
            UNUserNotificationCenter.Current.RemoveAllDeliveredNotifications();
        }

        /// <inheritdoc />
        public void RemoveNotification(string tag, int id)
        {
            this.RemoveNotification(id);
        }

        /// <inheritdoc />
        public async void RemoveNotification(int id)
        {
            const string notificationIdKey = "id";
            var deliveredNotifications = await UNUserNotificationCenter.Current.GetDeliveredNotificationsAsync();
            var deliveredNotificationsMatches = deliveredNotifications
                .Where(u => $"{u.Request.Content.UserInfo[notificationIdKey]}".Equals($"{id}"))
                .Select(s => s.Request.Identifier)
                .ToArray();
            if (deliveredNotificationsMatches.Length > 0)
            {
                UNUserNotificationCenter.Current.RemoveDeliveredNotifications(deliveredNotificationsMatches);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (UNUserNotificationCenter.Current.Delegate is UNUserNotificationCenterDelegateImpl)
                    {
                        UNUserNotificationCenter.Current.Delegate = null;
                    }

                    if (Firebase.CloudMessaging.Messaging.SharedInstance.Delegate is MessagingDelegateImpl)
                    {
                        Firebase.CloudMessaging.Messaging.SharedInstance.Delegate = null;
                    }
                }

                this.disposed = true;
            }
        }
    }
}