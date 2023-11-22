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
        private HashSet<string> subscribedTopics;

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

        /// <inheritdoc />
        public IEnumerable<NotificationChannelRequest> NotificationChannels { get; private set; }

        private static readonly NotificationChannelRequest DefaultNotificationChannel = new NotificationChannelRequest
        {
            ChannelId = Constants.DefaultNotificationChannelId,
            ChannelName = Constants.DefaultNotificationChannelName,
            IsDefault = true,
        };

        protected override void ConfigurePlatform(FirebasePushNotificationOptions options)
        {
            NotificationActivityType = options.Android.NotificationActivityType;
            //DefaultNotificationChannelId = options.Android.DefaultNotificationChannelId;

            var notificationChannels = options.Android.NotificationChannels;
            var duplicateChannelIds = notificationChannels
                .Select(c => c.ChannelId).Concat(new[] { DefaultNotificationChannel.ChannelId })
                .GroupBy(c => c)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicateChannelIds.Any())
            {
                throw new ArgumentException(
                 $"FirebasePushNotificationOptions contains {nameof(NotificationChannelRequest)} with duplicate {nameof(NotificationChannelRequest.ChannelId)}: " +
                 $"[{string.Join(", ", duplicateChannelIds.Select(id => $"\"{id}\""))}]",
                 nameof(FirebasePushNotificationAndroidOptions.NotificationChannels));
            }

            if (notificationChannels.Length == 0)
            {
                notificationChannels = new[] { DefaultNotificationChannel };
                StaticNotificationChannels.UpdateChannels(notificationChannels);
            }
            else
            {
                var defaultNotificationChannels = notificationChannels.Where(c => c.IsDefault).ToArray();
                if (defaultNotificationChannels.Length > 1)
                {
                    throw new ArgumentException(
                        $"FirebasePushNotificationOptions contains more than one {nameof(NotificationChannelRequest)} with {nameof(NotificationChannelRequest.IsDefault)}=true: " +
                        $"[{string.Join(", ", defaultNotificationChannels.Select(c => $"\"{c.ChannelId}\""))}]",
                        nameof(FirebasePushNotificationAndroidOptions.NotificationChannels));
                }
                else if (defaultNotificationChannels.Length < 1)
                {
                    throw new ArgumentException(
                        $"FirebasePushNotificationOptions does not contain any {nameof(NotificationChannelRequest)} with {nameof(NotificationChannelRequest.IsDefault)}=true",
                        nameof(FirebasePushNotificationAndroidOptions.NotificationChannels));
                }

                StaticNotificationChannels.UpdateChannels(notificationChannels);
            }

            this.NotificationChannels = notificationChannels;


            // TODO: REmove this again!!
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
                    var prefs = Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private);

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
                    this.UpdateToken(token);
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

                await Task.Run(this.DeleteTokenAsync);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UnregisterForPushNotificationsAsync failed with exception");
                this.HandleNotificationError(FirebasePushNotificationErrorType.UnregistrationFailed, ex.ToString());
            }
        }

        private async Task DeleteTokenAsync()
        {
            this.logger.LogDebug("DeleteTokenAsync");

            try
            {
                var tcs = new TaskCompletionSource<Java.Lang.Object>();
                var taskCompleteListener = new TaskCompleteListener(tcs);
                FirebaseMessaging.Instance.DeleteToken().AddOnCompleteListener(taskCompleteListener);

                await tcs.Task;

                this.RemoveToken();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "DeleteTokenAsync failed with exception");
            }
        }

        /// <inheritdoc />
        public string Token
        {
            get
            {
                // TODO: Read FirebaseTokenKey in a central place (now it's spread all over the code
                using (var sharedPreferences = Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private))
                {
                    return sharedPreferences.GetString(Constants.FirebaseTokenKey, null);
                }
            }
        }

        /// <inheritdoc />
        public string[] SubscribedTopics
        {
            get
            {
                if (this.subscribedTopics == null)
                {
                    using (var sharedPreferences = Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private))
                    {
                        var topicsSettingsValue = sharedPreferences.GetStringSet(Constants.FirebaseTopicsKey, null) ?? Array.Empty<string>();
                        this.subscribedTopics = new HashSet<string>(topicsSettingsValue);
                    }
                }

                return this.subscribedTopics.ToArray();
            }
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

            foreach (var notificationCategory in notificationCategories)
            {
                this.notificationCategories.Add(notificationCategory);
            }
        }

        /// <inheritdoc />
        public void Subscribe(string[] topics)
        {
            foreach (var t in topics)
            {
                this.Subscribe(t);
            }
        }

        /// <inheritdoc />
        public void Subscribe(string topic)
        {
            if (topic == null)
            {
                throw new ArgumentNullException(nameof(topic), "Topic must not be null");
            }

            if (topic == string.Empty)
            {
                throw new ArgumentException("Topic must not be empty", nameof(topic));
            }

            this.logger.LogDebug($"Subscribe: topic=\"{topic}\"");

            var subscribedTopics = new HashSet<string>(this.SubscribedTopics);
            if (!subscribedTopics.Contains(topic))
            {
                // TODO: Use AddOnCompleteListener(...)
                FirebaseMessaging.Instance.SubscribeToTopic(topic);

                subscribedTopics.Add(topic);

                using (var editor = Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).Edit())
                {
                    editor.PutStringSet(Constants.FirebaseTopicsKey, subscribedTopics);
                    editor.Commit();
                }

                this.subscribedTopics = subscribedTopics;
            }
            else
            {
                this.logger.LogInformation($"Subscribe ignored topic \"{topic}\"; topic is already subscribed");
            }
        }

        /// <inheritdoc />
        public void Unsubscribe(string[] topics)
        {
            foreach (var t in topics)
            {
                this.Unsubscribe(t);
            }
        }

        /// <inheritdoc />
        public void UnsubscribeAll()
        {
            foreach (var topic in this.SubscribedTopics)
            {
                // TODO: Use AddOnCompleteListener(...)
                FirebaseMessaging.Instance.UnsubscribeFromTopic(topic);
            }

            this.subscribedTopics.Clear();

            // TODO: Unify access to preferences
            var editor = Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).Edit();
            editor.PutStringSet(Constants.FirebaseTopicsKey, this.subscribedTopics);
            editor.Commit();
        }

        /// <inheritdoc />
        public void Unsubscribe(string topic)
        {
            if (topic == null)
            {
                throw new ArgumentNullException(nameof(topic), "Topic must not be null");
            }

            if (topic == string.Empty)
            {
                throw new ArgumentException("Topic must not be empty", nameof(topic));
            }

            this.logger.LogDebug($"Unsubscribe: topic=\"{topic}\"");

            var subscribedTopics = new HashSet<string>(this.SubscribedTopics);
            if (subscribedTopics.Contains(topic))
            {
                // TODO: Use AddOnCompleteListener(...)
                FirebaseMessaging.Instance.UnsubscribeFromTopic(topic);
                subscribedTopics.Remove(topic);

                // TODO: Unify access to preferences
                var editor = Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).Edit();
                editor.PutStringSet(Constants.FirebaseTopicsKey, subscribedTopics);
                editor.Commit();

                this.subscribedTopics = subscribedTopics;
            }
            else
            {
                this.logger.LogInformation($"Unsubscribe ignored topic \"{topic}\"; topic is not subscribed");
            }
        }

        protected override void OnTokenRefresh(string token)
        {
            this.UpdateToken(token);
        }

        private void UpdateToken(string token)
        {
            this.logger.LogDebug("UpdateToken");

            using (var editor = Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).Edit())
            {
                editor.PutString(Constants.FirebaseTokenKey, token);
                editor.Commit();
            }
        }

        private void RemoveToken()
        {
            this.logger.LogDebug("RemoveToken");

            using (var editor = Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).Edit())
            {
                editor.Remove(Constants.FirebaseTokenKey);
                editor.Commit();
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
