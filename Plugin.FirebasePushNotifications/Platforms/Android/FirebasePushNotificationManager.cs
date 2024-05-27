using Android.App;
using Android.Content;
using Firebase.Messaging;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Extensions;
using Plugin.FirebasePushNotifications.Platforms.Channels;
using Application = Android.App.Application;

namespace Plugin.FirebasePushNotifications.Platforms
{
    /// <summary>
    /// Implementation of <see cref="IFirebasePushNotification"/>
    /// for Android.
    /// </summary>
    public partial class FirebasePushNotificationManager : FirebasePushNotificationManagerBase, IFirebasePushNotification
    {
        public static string NotificationContentTitleKey { get; set; }
        public static string NotificationContentTextKey { get; set; }
        public static string NotificationContentDataKey { get; set; }
        public static int IconResource { get; set; }
        public static int LargeIconResource { get; set; }
        public static bool ShouldShowWhen { get; set; } = true;
        public static bool UseBigTextStyle { get; set; } = true;
        public static Android.Net.Uri SoundUri { get; set; }
        public static Android.Graphics.Color? Color { get; set; }
        public static Type NotificationActivityType { get; set; }
        public static ActivityFlags? NotificationActivityFlags { get; set; } = ActivityFlags.ClearTop | ActivityFlags.SingleTop;
        public static NotificationImportance DefaultNotificationChannelImportance { get; set; } = NotificationImportance.Default;

        internal static Type DefaultNotificationActivityType { get; set; } = null;

        public FirebasePushNotificationManager()
            : base()
        {
        }

        protected override void ConfigurePlatform(FirebasePushNotificationOptions options)
        {
            NotificationActivityType = options.Android.NotificationActivityType;

            var notificationChannels = NotificationChannels.Current;
            notificationChannels.CreateChannels(options.Android.NotificationChannels);

            // TODO: Migrate this code into Android-specific FirebasePushNotificationManager class.
            this.NotificationHandler = new DefaultPushNotificationHandler();
        }

        public void ProcessIntent(Activity activity, Intent intent)
        {
            if (activity == null)
            {
                return;
            }

            var activityType = activity.GetType();

            // Initialize NotificationActivityType in case it was left null
            // in FirebasePushNotificationOptions.Android.NotificationActivityType.
            if (NotificationActivityType == null && typeof(MauiAppCompatActivity).IsAssignableFrom(activityType))
            {
                NotificationActivityType = activityType;
            }

            if (NotificationActivityType != activityType)
            {
                return;
            }

            if (intent == null)
            {
                return;
            }

            var extras = intent.GetExtrasDict();
            this.logger.LogDebug($"ProcessIntent: activity.Type={activityType.Name}, intent.Flags=[{intent.Flags}], intent.Extras=[{extras.ToDebugString()}]");

            var launchedFromHistory = intent.Flags.HasFlag(ActivityFlags.LaunchedFromHistory);
            if (launchedFromHistory)
            {
                // Don't process the intent if flag FLAG_ACTIVITY_LAUNCHED_FROM_HISTORY is present
                return;
            }

            if (extras.Any())
            {
                // Don't process old/historic intents which are recycled for whatever reasons
                var intentAlreadyHandledKey = Constants.ExtraFirebaseProcessIntentHandled;
                if (!intent.GetBooleanExtra(intentAlreadyHandledKey, false))
                {
                    intent.PutExtra(intentAlreadyHandledKey, true);
                    this.logger.LogDebug($"ProcessIntent: {intentAlreadyHandledKey} not present --> Process notification");

                    // TODO: Refactor this! This is for sure not a good behavior..
                    DefaultNotificationActivityType = activityType;

                    var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;

                    if (extras.TryGetInt(Constants.ActionNotificationIdKey, out var notificationId))
                    {
                        if (extras.TryGetString(Constants.ActionNotificationTagKey, out var notificationTag))
                        {
                            notificationManager.Cancel(notificationTag, notificationId);
                        }
                        else
                        {
                            notificationManager.Cancel(notificationId);
                        }
                    }

                    // TODO: Pass object instead of 3 parameters
                    var notificationActionId = extras.GetStringOrDefault(Constants.NotificationActionId);
                    if (notificationActionId == null)
                    {
                        this.HandleNotificationOpened(extras, notificationActionId, NotificationCategoryType.Default);
                    }
                    else
                    {
                        this.HandleNotificationAction(extras, notificationActionId, NotificationCategoryType.Default);
                    }
                }
                else
                {
                    this.logger.LogDebug($"ProcessIntent: {intentAlreadyHandledKey} is present --> Notification already processed");
                }
            }
        }

        /// <inheritdoc />
        public async Task RegisterForPushNotificationsAsync()
        {
            this.logger.LogDebug("RegisterForPushNotificationsAsync");

            try
            {
                FirebaseMessaging.Instance.AutoInitEnabled = true;

                await Task.Run(this.GetTokenAsync);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "RegisterForPushNotificationsAsync failed with exception");
                throw;
            }
        }

        private async Task GetTokenAsync()
        {
            var tcs = new TaskCompletionSource<Java.Lang.Object>();
            var taskCompleteListener = new TaskCompleteListener(tcs);
            FirebaseMessaging.Instance.GetToken().AddOnCompleteListener(taskCompleteListener);

            try
            {
                var taskResult = await tcs.Task;
                var token = taskResult.ToString();

                if (!string.IsNullOrEmpty(token))
                {
                    this.preferences.Set(Constants.Preferences.TokenKey, token);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "GetTokenAsync failed with exception");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task UnregisterForPushNotificationsAsync()
        {
            this.logger.LogDebug("UnregisterForPushNotificationsAsync");

            try
            {
                FirebaseMessaging.Instance.AutoInitEnabled = false;

                await Task.Run(async () =>
                {
                    var tcs = new TaskCompletionSource<Java.Lang.Object>();
                    var taskCompleteListener = new TaskCompleteListener(tcs);
                    FirebaseMessaging.Instance.DeleteToken().AddOnCompleteListener(taskCompleteListener);

                    await tcs.Task;
                }
                );
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UnregisterForPushNotificationsAsync failed with exception");
                throw;
            }
            finally
            {
                this.preferences.Remove(Constants.Preferences.TokenKey);
            }
        }

        /// <inheritdoc />
        public string Token
        {
            get
            {
                return this.preferences.Get<string>(Constants.Preferences.TokenKey);
            }
            //private set
            //{

            //}
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

            var subscribedTopics = new HashSet<string>(this.SubscribedTopics);
            if (!subscribedTopics.Contains(topic))
            {
                this.logger.LogDebug($"Subscribe: topic=\"{topic}\"");

                // TODO: Use AddOnCompleteListener(...)
                FirebaseMessaging.Instance.SubscribeToTopic(topic);

                subscribedTopics.Add(topic);

                // TODO: Improve write performance here; don't loop all topics one by one
                this.SubscribedTopics = subscribedTopics.ToArray();
            }
            else
            {
                this.logger.LogInformation($"Subscribe: skipping topic \"{topic}\"; topic is already subscribed");
            }
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
        public void UnsubscribeAllTopics()
        {
            foreach (var topic in this.SubscribedTopics)
            {
                // TODO: Use AddOnCompleteListener(...)
                FirebaseMessaging.Instance.UnsubscribeFromTopic(topic);
            }

            this.SubscribedTopics = null;
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

            var subscribedTopics = new HashSet<string>(this.SubscribedTopics);
            if (subscribedTopics.Contains(topic))
            {
                this.logger.LogDebug($"Unsubscribe: topic=\"{topic}\"");

                // TODO: Use AddOnCompleteListener(...)
                FirebaseMessaging.Instance.UnsubscribeFromTopic(topic);
                subscribedTopics.Remove(topic);

                // TODO: Improve write performance here; don't loop all topics one by one
                this.SubscribedTopics = subscribedTopics.ToArray();
            }
            else
            {
                this.logger.LogInformation($"Unsubscribe: skipping topic \"{topic}\"; topic is not subscribed");
            }
        }

        protected override void HandleTokenRefreshPlatform(string token)
        {
            this.ResubscribeExistingTopics();
        }

        /// <summary>
        /// Resubscribes all existing topics since the old instance id isn't valid anymore.
        /// This is obviously necessary but seems a very bad design decision...
        /// </summary>
        private void ResubscribeExistingTopics()
        {
            foreach (var topic in this.SubscribedTopics)
            {
                // TODO: Use AddOnCompleteListener(...)
                FirebaseMessaging.Instance.SubscribeToTopic(topic);
            }
        }

        /// <inheritdoc />
        protected override void ClearAllNotificationsPlatform()
        {
            var manager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            manager.CancelAll();
        }

        /// <inheritdoc />
        public void RemoveNotification(int id)
        {
            var manager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            manager.Cancel(id);
        }

        /// <inheritdoc />
        public void RemoveNotification(string tag, int id)
        {
            if (string.IsNullOrEmpty(tag))
            {
                this.RemoveNotification(id);
            }
            else
            {
                var manager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
                manager.Cancel(tag, id);
            }
        }
    }
}
