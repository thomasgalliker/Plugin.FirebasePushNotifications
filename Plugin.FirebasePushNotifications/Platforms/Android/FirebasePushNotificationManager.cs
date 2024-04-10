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

            // TODO: Remove this again!!
            this.NotificationHandler = new DefaultPushNotificationHandler();
        }

        public void ProcessIntent(Activity activity, Intent intent)
        {
            if (activity == null)
            {
                this.logger.LogDebug($"ProcessIntent: activity=null");
                return;
            }

            var activityType = activity.GetType();

            if (intent == null)
            {
                this.logger.LogDebug($"ProcessIntent: activity.Type={activityType.Name}, intent=null");
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

        /*
        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Initialize(Context context, bool resetToken, bool createDefaultNotificationChannel = true, bool autoRegistration = true)
        {
            this.NotificationHandler ??= new DefaultPushNotificationHandler();
            FirebaseMessaging.Instance.AutoInitEnabled = autoRegistration;
            if (autoRegistration)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {

                    var packageInfo = Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, PackageInfoFlags.MetaData);
                    var packageName = packageInfo.PackageName;
                    var versionCode = packageInfo.VersionCode;
                    var versionName = packageInfo.VersionName;
                    var prefs = Android.App.Application.Context.GetSharedPreferences(Constants.Preferences.KeyGroupName, FileCreationMode.Private);

                    try
                    {
                        var storedVersionName = prefs.GetString(AppVersionNameKey, string.Empty);
                        var storedVersionCode = prefs.GetString(AppVersionCodeKey, string.Empty);
                        var storedPackageName = prefs.GetString(AppVersionPackageNameKey, string.Empty);

                        if (resetToken || (!string.IsNullOrEmpty(storedPackageName) && (!storedPackageName.Equals(packageName, StringComparison.CurrentCultureIgnoreCase) || !storedVersionName.Equals(versionName, StringComparison.CurrentCultureIgnoreCase) || !storedVersionCode.Equals($"{versionCode}", StringComparison.CurrentCultureIgnoreCase))))
                        {
                            this.CleanUp(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Initialize failed with exception");
                        this.HandleNotificationError(FirebasePushNotificationErrorType.Unknown, ex.ToString());
                    }
                    finally
                    {
                        var editor = prefs.Edit();
                        editor.PutString(AppVersionNameKey, $"{versionName}");
                        editor.PutString(AppVersionCodeKey, $"{versionCode}");
                        editor.PutString(AppVersionPackageNameKey, $"{packageName}");
                        editor.Commit();
                    }

                    _ = CrossFirebasePushNotification.Current.RegisterForPushNotificationsAsync();
                });
            }

#if ANDROID26_0_OR_GREATER
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O && createDefaultNotificationChannel)
            {
                // Create channel to show notifications.
                var channelId = DefaultNotificationChannelId;
                var channelName = DefaultNotificationChannelName;
                var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);

                var defaultSoundUri = SoundUri ?? RingtoneManager.GetDefaultUri(RingtoneType.Notification);
                var attributes = new AudioAttributes.Builder()
                    .SetUsage(AudioUsageKind.Notification)
                    .SetContentType(AudioContentType.Sonification)
                    .SetLegacyStreamType(Android.Media.Stream.Notification)
                    .Build();

                var notificationChannel = new NotificationChannel(channelId, channelName, DefaultNotificationChannelImportance);
                notificationChannel.EnableLights(true);
                notificationChannel.SetSound(defaultSoundUri, attributes);

                notificationManager.CreateNotificationChannel(notificationChannel);
            }
#endif
        }

        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Initialize(Context context, NotificationUserCategory[] notificationCategories, bool resetToken, bool createDefaultNotificationChannel = true, bool autoRegistration = true)
        {
            this.Initialize(context, resetToken, createDefaultNotificationChannel, autoRegistration);
            this.RegisterUserNotificationCategories(notificationCategories);
        }
        
        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Initialize(Context context, IPushNotificationHandler pushNotificationHandler, bool resetToken, bool createDefaultNotificationChannel = true, bool autoRegistration = true)
        {
            this.NotificationHandler = pushNotificationHandler;
            this.Initialize(context, resetToken, createDefaultNotificationChannel, autoRegistration);
        }

         */

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
                this.HandleNotificationError(FirebasePushNotificationErrorType.RegistrationFailed, ex.ToString());
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
                this.HandleNotificationError(FirebasePushNotificationErrorType.RegistrationFailed, $"{ex}");
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
                this.HandleNotificationError(FirebasePushNotificationErrorType.UnregistrationFailed, ex.ToString());
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

        //public void SendDeviceGroupMessage(IDictionary<string, string> parameters, string groupKey, string messageId, int timeOfLive)
        //{
        //    var message = new RemoteMessage.Builder(groupKey);
        //    message.SetData(parameters);
        //    message.SetMessageId(messageId);
        //    message.SetTtl(timeOfLive);
        //    FirebaseMessaging.Instance.Send(message.Build());
        //}

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
        public void ClearAllNotifications()
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
