﻿using System.Runtime.Versioning;
using Firebase.CloudMessaging;
using Foundation;
using Microsoft.Extensions.Logging;
using UIKit;
using UserNotifications;

namespace Plugin.FirebasePushNotifications.Platforms
{
    /// <summary>
    /// Implementation of <see cref="IFirebasePushNotification"/>
    /// for iOS.
    /// </summary>
    public partial class FirebasePushNotificationManager : FirebasePushNotificationManagerBase, IFirebasePushNotification
    {
        private readonly Queue<Tuple<string, bool>> pendingTopics = new Queue<Tuple<string, bool>>();
        private bool hasToken = false;

        private readonly NSMutableArray currentTopics = (NSUserDefaults.StandardUserDefaults.ValueForKey(Constants.FirebaseTopicsKey) as NSArray ?? new NSArray()).MutableCopy() as NSMutableArray;

        private bool disposed;

        public FirebasePushNotificationManager()
            : base()
        {
        }

        public string Token
        {
            get
            {
                var fcmToken = Messaging.SharedInstance.FcmToken;
                if (!string.IsNullOrEmpty(fcmToken))
                {
                    return fcmToken;
                }
                else
                {
                    return NSUserDefaults.StandardUserDefaults.StringForKey(Constants.FirebaseTokenKey);
                }
            }
        }

        /// <inheritdoc />
        public string[] SubscribedTopics
        {
            get
            {
                //Load all subscribed topics
                var topics = new List<string>();

                for (nuint i = 0; i < this.currentTopics.Count; i++)
                {
                    topics.Add(this.currentTopics.GetItem<NSString>(i));
                }

                return topics.ToArray();
            }
        }

        //[Obsolete]
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public void Initialize(NSDictionary options, bool autoRegistration = true)
        //{
        //    if (App.DefaultInstance == null)
        //    {
        //        App.Configure();
        //    }

        //    this.NotificationHandler ??= new DefaultPushNotificationHandler();
        //    Messaging.SharedInstance.AutoInitEnabled = autoRegistration;

        //    if (options?.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey) ?? false)
        //    {
        //        if (options[UIApplication.LaunchOptionsRemoteNotificationKey] is NSDictionary pushPayload)
        //        {
        //            var parameters = GetParameters(pushPayload);
        //            // TODO: Pass single object instead of 3 parameters
        //            this.HandleNotificationOpened(parameters, null, NotificationCategoryType.Default);
        //        }
        //    }

        //    if (autoRegistration)
        //    {
        //        _ = this.RegisterForPushNotificationsAsync();
        //    }
        //}

        //[Obsolete]
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public void Initialize(NSDictionary options, IPushNotificationHandler pushNotificationHandler, bool autoRegistration = true)
        //{
        //    this.NotificationHandler = pushNotificationHandler;
        //    this.Initialize(options, autoRegistration);
        //}

        //[Obsolete]
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public void Initialize(NSDictionary options, NotificationUserCategory[] notificationUserCategories, bool autoRegistration = true)
        //{
        //    this.Initialize(options, autoRegistration);

        //    this.RegisterUserNotificationCategories(notificationUserCategories);
        //}

        /// <inheritdoc />
        public void RegisterNotificationCategories(NotificationCategory[] notificationCategories)
        {
            if (notificationCategories == null)
            {
                throw new ArgumentNullException(nameof(notificationCategories));
            }

            if (notificationCategories.Length == 0)
            {
                throw new ArgumentException($"{nameof(notificationCategories)} must not be empty", nameof(notificationCategories));
            }

            this.ClearNotificationCategories();

            if (notificationCategories.Length > 0)
            {
                var categories = new List<UNNotificationCategory>();
                foreach (var notificationCategory in notificationCategories)
                {
                    var notificationActions = new List<UNNotificationAction>();

                    foreach (var action in notificationCategory.Actions)
                    {
                        var notificationActionType = GetUNNotificationActionOptions(action.Type);
                        var notificationAction = UNNotificationAction.FromIdentifier(action.Id, action.Title, notificationActionType);
                        notificationActions.Add(notificationAction);
                    }

                    // Create category
                    var options = notificationCategory.Type == NotificationCategoryType.Dismiss
                        ? UNNotificationCategoryOptions.CustomDismissAction
                        : UNNotificationCategoryOptions.None;

                    var category = UNNotificationCategory.FromIdentifier(
                        identifier: notificationCategory.CategoryId,
                        actions: notificationActions.ToArray(),
                        intentIdentifiers: Array.Empty<string>(),
                        options);
                    categories.Add(category);

                    this.notificationCategories.Add(notificationCategory);
                }

                // Register categories
                var notificationCategoriesSet = new NSSet<UNNotificationCategory>(categories.ToArray());
                UNUserNotificationCenter.Current.SetNotificationCategories(notificationCategoriesSet);
            }
        }

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

        protected override void ConfigurePlatform(FirebasePushNotificationOptions options)
        {
            if (Firebase.CloudMessaging.Messaging.SharedInstance.Delegate != null)
            {
                this.logger.LogWarning("Firebase.CloudMessaging.Messaging.SharedInstance.Delegate is already set");
            }
            else
            {
                Firebase.CloudMessaging.Messaging.SharedInstance.Delegate = new MessagingDelegateImpl(this.DidReceiveRegistrationToken);
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

            if (options.AutoInitEnabled)
            {
                Messaging.SharedInstance.AutoInitEnabled = true;
            }
        }

        /// <inheritdoc />
        public async Task RegisterForPushNotificationsAsync()
        {
            this.logger.LogDebug("RegisterForPushNotificationsAsync");

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                Messaging.SharedInstance.AutoInitEnabled = true;

                var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
                var (granted, error) = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(authOptions);
                if (error != null)
                {
                    this.HandleNotificationError(FirebasePushNotificationErrorType.PermissionDenied, error.Description);
                }
                else if (!granted)
                {
                    this.HandleNotificationError(FirebasePushNotificationErrorType.PermissionDenied, "Push notification permission not granted");
                }
                else
                {
                    await MainThread.InvokeOnMainThreadAsync(UIApplication.SharedApplication.RegisterForRemoteNotifications);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc />
        public Task UnregisterForPushNotificationsAsync()
        {
            this.logger.LogDebug("UnregisterForPushNotificationsAsync");

            if (this.hasToken)
            {
                this.UnsubscribeAll();
                this.hasToken = false;
            }

            Messaging.SharedInstance.AutoInitEnabled = false;
            UIApplication.SharedApplication.UnregisterForRemoteNotifications();
            NSUserDefaults.StandardUserDefaults.SetString(string.Empty, Constants.FirebaseTokenKey);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void RegisteredForRemoteNotifications(NSData deviceToken)
        {
            this.logger.LogDebug("RegisteredForRemoteNotifications");

            Messaging.SharedInstance.ApnsToken = deviceToken;
        }

        /// <inheritdoc />
        public void FailedToRegisterForRemoteNotifications(NSError error)
        {
            this.logger.LogError(new NSErrorException(error), "FailedToRegisterForRemoteNotifications");

            this.HandleNotificationError(FirebasePushNotificationErrorType.RegistrationFailed, error.Description);
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
            Messaging.SharedInstance.AppDidReceiveMessage(userInfo);
            var data = GetParameters(userInfo);
            this.HandleNotificationReceived(data);
        }

        private void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            var data = GetParameters(notification.Request.Content.UserInfo);
            var notificationPresentationOptions = GetNotificationPresentationOptions(data);
            this.logger.LogDebug($"WillPresentNotification: UNNotificationPresentationOptions={notificationPresentationOptions}");

            this.HandleNotificationReceived(data);

            completionHandler(notificationPresentationOptions);
        }

        private static UNNotificationPresentationOptions GetNotificationPresentationOptions(IDictionary<string, object> data)
        {
            var notificationPresentationOptions = UNNotificationPresentationOptions.None;

            if (data.TryGetValue("priority", out var priority) && ($"{priority}".ToLower() is "high" or "max"))
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
            else if ($"{priority}".ToLower() is "default" or "low" or "min")
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

            return notificationPresentationOptions;
        }

        private static IDictionary<string, object> GetParameters(NSDictionary data)
        {
            var parameters = new Dictionary<string, object>();

            var keyAps = new NSString("aps");
            var keyAlert = new NSString("alert");

            foreach (var val in data)
            {
                if (val.Key.Equals(keyAps))
                {
                    if (data.ValueForKey(keyAps) is NSDictionary aps)
                    {
                        foreach (var apsVal in aps)
                        {
                            if (apsVal.Value is NSDictionary)
                            {
                                if (apsVal.Key.Equals(keyAlert))
                                {
                                    foreach (var alertVal in apsVal.Value as NSDictionary)
                                    {
                                        parameters.Add($"aps.alert.{alertVal.Key}", $"{alertVal.Value}");
                                    }
                                }
                            }
                            else
                            {
                                parameters.Add($"aps.{apsVal.Key}", $"{apsVal.Value}");
                            }

                        }
                    }
                }
                else
                {
                    parameters.Add($"{val.Key}", $"{val.Value}");
                }
            }

            return parameters;
        }

        public void Subscribe(string[] topics)
        {
            foreach (var t in topics)
            {
                this.Subscribe(t);
            }
        }

        public void Subscribe(string topic)
        {
            if (!this.hasToken)
            {
                this.pendingTopics.Enqueue(new Tuple<string, bool>(topic, true));
                return;
            }

            if (!this.currentTopics.Contains(new NSString(topic)))
            {
                Messaging.SharedInstance.Subscribe($"{topic}");
                this.currentTopics.Add(new NSString(topic));
            }

            NSUserDefaults.StandardUserDefaults.SetValueForKey(this.currentTopics, Constants.FirebaseTopicsKey);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        public void UnsubscribeAll()
        {
            for (nuint i = 0; i < this.currentTopics.Count; i++)
            {
                this.Unsubscribe(this.currentTopics.GetItem<NSString>(i));
            }
        }

        public void Unsubscribe(string[] topics)
        {
            foreach (var t in topics)
            {
                this.Unsubscribe(t);
            }
        }

        public void Unsubscribe(string topic)
        {
            if (!this.hasToken)
            {
                this.pendingTopics.Enqueue(new Tuple<string, bool>(topic, false));
                return;
            }

            var deletedKey = new NSString(topic);
            if (this.currentTopics.Contains(deletedKey))
            {
                Messaging.SharedInstance.Unsubscribe(topic);
                var idx = (nint)this.currentTopics.IndexOf(deletedKey);
                if (idx != -1)
                {
                    this.currentTopics.RemoveObject(idx);
                }
            }

            NSUserDefaults.StandardUserDefaults.SetValueForKey(this.currentTopics, Constants.FirebaseTopicsKey);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        //public void SendDeviceGroupMessage(IDictionary<string, string> parameters, string groupKey, string messageId, int timeOfLive)
        //{
        //    if (hasToken)
        //    {
        //        using (var message = new NSMutableDictionary())
        //        {
        //            foreach (var p in parameters)
        //            {
        //                message.Add(new NSString(p.Key), new NSString(p.Value));
        //            }

        //            Messaging.SharedInstance.SendMessage(message, groupKey, messageId, timeOfLive);
        //        }

        //    }
        //}

        private void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            this.logger.LogDebug("DidReceiveNotificationResponse");

            var data = GetParameters(response.Notification.Request.Content.UserInfo);

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

            var actionIdentifier = $"{response.ActionIdentifier}";
            var identifier = actionIdentifier.Equals("com.apple.UNNotificationDefaultActionIdentifier", StringComparison.InvariantCultureIgnoreCase)
                ? null
                : actionIdentifier;

            if (string.IsNullOrEmpty(identifier))
            {
                this.HandleNotificationOpened(data, identifier, notificationCategoryType);
            }
            else
            {
                this.HandleNotificationAction(data, identifier, notificationCategoryType);
            }

            // Inform caller it has been handled
            completionHandler();
        }

        private void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            this.logger.LogDebug("DidReceiveRegistrationToken");

            // Note that this callback will be fired everytime a new token is generated,
            // including the first time a token is received.

            if (!string.IsNullOrEmpty(fcmToken))
            {
                this.HandleTokenRefresh(fcmToken);

                this.hasToken = true;

                while (this.pendingTopics.TryDequeue(out var pendingTopic))
                {
                    if (pendingTopic != null)
                    {
                        if (pendingTopic.Item2)
                        {
                            this.Subscribe(pendingTopic.Item1);
                        }
                        else
                        {
                            this.Unsubscribe(pendingTopic.Item1);
                        }
                    }
                }
            }

            NSUserDefaults.StandardUserDefaults.SetString(fcmToken ?? string.Empty, Constants.FirebaseTokenKey);
        }

        public void ClearAllNotifications()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                UNUserNotificationCenter.Current.RemoveAllDeliveredNotifications(); // To remove all delivered notifications
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void RemoveNotification(string tag, int id)
        {
            this.RemoveNotification(id);
        }

        public async void RemoveNotification(int id)
        {
            var NotificationIdKey = "id";
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                var deliveredNotifications = await UNUserNotificationCenter.Current.GetDeliveredNotificationsAsync();
                var deliveredNotificationsMatches = deliveredNotifications.Where(u => $"{u.Request.Content.UserInfo[NotificationIdKey]}".Equals($"{id}")).Select(s => s.Request.Identifier).ToArray();
                if (deliveredNotificationsMatches.Length > 0)
                {
                    UNUserNotificationCenter.Current.RemoveDeliveredNotifications(deliveredNotificationsMatches);

                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

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
                }

                // TODO: set large fields to null
                this.disposed = true;
            }
        }
    }
}
