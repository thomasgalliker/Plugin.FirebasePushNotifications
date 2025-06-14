using Android.App;
using Android.Content;
using Firebase;
using Firebase.Messaging;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Extensions;
using Application = Android.App.Application;

namespace Plugin.FirebasePushNotifications.Platforms
{
    /// <summary>
    /// Implementation of <see cref="IFirebasePushNotification"/>
    /// for Android.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class FirebasePushNotificationManager : FirebasePushNotificationManagerBase, IFirebasePushNotification
    {
        private readonly INotificationChannels notificationChannels;

        internal FirebasePushNotificationManager(
            ILogger<FirebasePushNotificationManager> logger,
            ILoggerFactory loggerFactory,
            FirebasePushNotificationOptions options,
            IPushNotificationHandler pushNotificationHandler,
            IFirebasePushNotificationPreferences preferences,
            INotificationChannels notificationChannels,
            INotificationBuilder notificationBuilder)
            : base(logger, loggerFactory, options, pushNotificationHandler, preferences)
        {
            this.notificationChannels = notificationChannels;
            this.NotificationBuilder = notificationBuilder;
            this.ConfigurePlatform();

            // There are several different versions in firebase-android-sdk.
            // We return the Firebase Cloud Messaging version here.
            this.SdkVersion = Firebase.Messaging.BuildConfig.VersionName;
        }

        /// <inheritdoc />
        public string SdkVersion { get; }

        private void ConfigurePlatform()
        {
            this.logger.LogDebug("ConfigurePlatform");

            var groups = this.options.Android.NotificationChannelGroups;
            if (groups.Any())
            {
                this.notificationChannels.SetNotificationChannelGroups(groups);
            }

            var channels = this.options.Android.NotificationChannels;
            if (channels.Any())
            {
                // If we have NotificationChannels set, use them to configure the absolute set of notification channels.
                this.notificationChannels.SetNotificationChannels(channels);
            }
            else
            {
                // Otherwise, ensure we have at least the default notification channel.
                ((Channels.NotificationChannels)this.notificationChannels).EnsureDefaultNotificationChannel();
            }

            var context = Application.Context;
            var isFirebaseAppInitialized = FirebaseAppHelper.IsFirebaseAppInitialized(context);

            if (this.logger.IsEnabled(LogLevel.Debug))
            {
                this.logger.LogDebug($"ConfigurePlatform: " +
                                     $"isFirebaseAppInitialized={isFirebaseAppInitialized}, " +
                                     $"apps={{{string.Join(",", FirebaseApp.GetApps(context).Select(a => a.Name))}}}");
            }

            if (isFirebaseAppInitialized)
            {
                if (this.options.Android.FirebaseOptions != null)
                {
                    this.logger.LogWarning("ConfigurePlatform: Firebase is already configured; Android.FirebaseOptions is not used!");
                }
            }
            else
            {
                if (this.options.Android.FirebaseOptions is not FirebaseOptions firebaseOptions)
                {
                    this.InitializeFirebaseAppFromServiceFile(context);
                }
                else
                {
                    this.InitializeFirebaseAppFromFirebaseOptions(context, firebaseOptions);
                }
            }

            this.CheckIfFirebaseAppInitialized(context);

            FirebaseMessaging.Instance.AutoInitEnabled = this.options.AutoInitEnabled;
        }

        private void InitializeFirebaseAppFromServiceFile(Context context)
        {
            this.logger.LogDebug("InitializeFirebaseAppFromServiceFile");

            try
            {
                // Try to initialize Firebase from google-services.json.
                FirebaseApp.InitializeApp(context);
            }
            catch (Exception ex)
            {
                var exception = Exceptions.FailedToInitializeFirebaseApp(ex);
                this.logger.LogError(exception, "InitializeFirebaseAppFromServiceFile failed with exception");
                throw;
            }
        }

        private void CheckIfFirebaseAppInitialized(Context context)
        {
            var isInitialized = FirebaseAppHelper.IsFirebaseAppInitialized(context) &&
                                FirebaseApp.Instance != null;
            if (!isInitialized)
            {
                throw Exceptions.FailedToInitializeFirebaseApp();
            }
        }

        private void InitializeFirebaseAppFromFirebaseOptions(Context context, FirebaseOptions firebaseOptions)
        {
            this.logger.LogDebug("InitializeFirebaseAppFromFirebaseOptions");

            try
            {
                FirebaseApp.InitializeApp(context, firebaseOptions);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "InitializeFirebaseAppFromFirebaseOptions failed with exception");
                throw;
            }
        }

        public void ProcessIntent(Activity activity, Intent intent)
        {
            if (activity == null)
            {
                return;
            }

            var activityType = activity.GetType();

            if (this.options.Android.NotificationActivityType == null && typeof(MauiAppCompatActivity).IsAssignableFrom(activityType))
            {
                // Initialize NotificationActivityType in case it was left null
                // in FirebasePushNotificationAndroidOptions.NotificationActivityType.
                this.options.Android.NotificationActivityType = activityType;
            }

            if (this.options.Android.NotificationActivityType != activityType)
            {
                return;
            }

            if (intent == null)
            {
                return;
            }

            var launchedFromHistory = intent.Flags.HasFlag(ActivityFlags.LaunchedFromHistory);
            if (launchedFromHistory)
            {
                // Don't process the intent if flag FLAG_ACTIVITY_LAUNCHED_FROM_HISTORY is present
                return;
            }

            if (string.Equals(intent.Action, Intent.ActionMain, StringComparison.InvariantCultureIgnoreCase))
            {
                // Don't process the intent if intent action is android.intent.action.MAIN
                return;
            }

            var extras = intent.GetExtrasDict();
            this.logger.LogDebug(
                $"ProcessIntent: activity.Type={activityType.Name}, intent.Flags=[{intent.Flags}], intent.Extras=[{extras.ToDebugString()}]");

            if (extras.Any())
            {
                // Don't process old/historic intents which are recycled for whatever reason
                const string intentAlreadyHandledKey = Constants.ExtraFirebaseProcessIntentHandled;
                if (!intent.GetBooleanExtra(intentAlreadyHandledKey, false))
                {
                    intent.PutExtra(intentAlreadyHandledKey, true);
                    this.logger.LogDebug($"ProcessIntent: {intentAlreadyHandledKey} not present --> Process notification");

                    if (extras.TryGetInt(Constants.ActionNotificationIdKey, out var notificationId))
                    {
                        var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
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
                    var notificationCategoryId = extras.GetStringOrDefault(Constants.NotificationCategoryKey);
                    if (notificationCategoryId == null)
                    {
                        this.HandleNotificationOpened(extras, NotificationCategoryType.Default);
                    }
                    else
                    {
                        var notificationActionId = extras.GetStringOrDefault(Constants.NotificationActionId);
                        this.HandleNotificationAction(extras, notificationCategoryId, notificationActionId, NotificationCategoryType.Default);
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

                var tcs = new TaskCompletionSource<Java.Lang.Object>();
                var taskCompleteListener = new TaskCompleteListener(tcs);
                FirebaseMessaging.Instance.GetToken().AddOnCompleteListener(taskCompleteListener);

                var taskResult = await tcs.Task;
                var token = taskResult.ToString();

                if (!string.IsNullOrEmpty(token))
                {
                    this.preferences.Set(Constants.Preferences.TokenKey, token);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "RegisterForPushNotificationsAsync failed with exception");
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

                var tcs = new TaskCompletionSource<Java.Lang.Object>();
                var taskCompleteListener = new TaskCompleteListener(tcs);
                FirebaseMessaging.Instance.DeleteToken().AddOnCompleteListener(taskCompleteListener);

                await tcs.Task;
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

        public INotificationBuilder NotificationBuilder { get; set; }

        /// <inheritdoc />
        public async Task SubscribeTopicsAsync(string[] topics)
        {
            foreach (var topic in topics)
            {
                await this.SubscribeTopicAsync(topic);
            }
        }

        /// <inheritdoc />
        public async Task SubscribeTopicAsync(string topic)
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
                this.logger.LogDebug($"SubscribeTopicAsync: topic=\"{topic}\"");

                var tcs = new TaskCompletionSource<Java.Lang.Object>();
                var taskCompleteListener = new TaskCompleteListener(tcs);
                FirebaseMessaging.Instance.SubscribeToTopic(topic).AddOnCompleteListener(taskCompleteListener);
                await tcs.Task;

                subscribedTopics.Add(topic);

                // TODO: Improve write performance here; don't loop all topics one by one
                this.SubscribedTopics = subscribedTopics.ToArray();
            }
            else
            {
                this.logger.LogInformation($"SubscribeTopicAsync: skipping topic \"{topic}\"; topic is already subscribed");
            }
        }

        /// <inheritdoc />
        public async Task UnsubscribeTopicsAsync(string[] topics)
        {
            if (topics == null)
            {
                throw new ArgumentNullException(nameof(topics), $"Parameter '{nameof(topics)}' must not be null");
            }

            // TODO: Improve efficiency here (move to base class maybe)
            foreach (var topic in topics)
            {
                await this.UnsubscribeTopicAsync(topic);
            }
        }

        /// <inheritdoc />
        public async Task UnsubscribeAllTopicsAsync()
        {
            var topics = this.SubscribedTopics.ToArray();
            this.logger.LogDebug($"UnsubscribeAllTopicsAsync: topics=[{string.Join(',', topics)}]");

            foreach (var topic in this.SubscribedTopics)
            {
                var tcs = new TaskCompletionSource<Java.Lang.Object>();
                var taskCompleteListener = new TaskCompleteListener(tcs);
                FirebaseMessaging.Instance.UnsubscribeFromTopic(topic).AddOnCompleteListener(taskCompleteListener);
                await tcs.Task;
            }

            this.SubscribedTopics = null;
        }

        /// <inheritdoc />
        public async Task UnsubscribeTopicAsync(string topic)
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
                this.logger.LogDebug($"UnsubscribeTopicAsync: topic=\"{topic}\"");

                var tcs = new TaskCompletionSource<Java.Lang.Object>();
                var taskCompleteListener = new TaskCompleteListener(tcs);
                FirebaseMessaging.Instance.UnsubscribeFromTopic(topic).AddOnCompleteListener(taskCompleteListener);
                await tcs.Task;

                subscribedTopics.Remove(topic);

                // TODO: Improve write performance here; don't loop all topics one by one
                this.SubscribedTopics = subscribedTopics.ToArray();
            }
            else
            {
                this.logger.LogInformation($"UnsubscribeTopicAsync: skipping topic \"{topic}\"; topic is not subscribed");
            }
        }

        protected override void HandleTokenRefreshPlatform(string token)
        {
            _ = this.ResubscribeExistingTopicsAsync();
        }

        /// <summary>
        /// Resubscribes all existing topics since the old instance id isn't valid anymore.
        /// This is obviously necessary but seems a very bad design decision...
        /// </summary>
        private async Task ResubscribeExistingTopicsAsync()
        {
            foreach (var topic in this.SubscribedTopics)
            {
                var tcs = new TaskCompletionSource<Java.Lang.Object>();
                var taskCompleteListener = new TaskCompleteListener(tcs);
                FirebaseMessaging.Instance.SubscribeToTopic(topic).AddOnCompleteListener(taskCompleteListener);
                await tcs.Task;
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