using System.Collections.ObjectModel;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
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
    public partial class FirebasePushNotificationManager : FirebasePushNotificationManagerBase, IFirebasePushNotification
    {
        internal const string AppVersionCodeKey = "AppVersionCodeKey";
        internal const string AppVersionNameKey = "AppVersionNameKey";
        internal const string AppVersionPackageNameKey = "AppVersionPackageNameKey";

        // internal const string NotificationDeletedActionId = "Plugin.PushNotification.NotificationDeletedActionId";
        private readonly ICollection<string> currentTopics = new HashSet<string>(Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).GetStringSet(Constants.FirebaseTopicsKey, new Collection<string>()));
        private readonly IList<NotificationUserCategory> userNotificationCategories = new List<NotificationUserCategory>();
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

        public static string DefaultNotificationChannelId { get; set; } = "FirebasePushNotificationChannel";
        public static string DefaultNotificationChannelName { get; set; } = "General";
        public static NotificationImportance DefaultNotificationChannelImportance { get; set; } = NotificationImportance.Default;

        internal static Type DefaultNotificationActivityType { get; set; } = null;

        protected override void ConfigurePlatform(FirebasePushNotificationOptions options)
        {
            NotificationActivityType = options.Android.NotificationActivityType;
            DefaultNotificationChannelId = options.Android.DefaultNotificationChannelId;
        }

        public void ProcessIntent(Activity activity, Intent intent)
        {
            if (activity == null)
            {
                this.logger.LogDebug($"ProcessIntent: activity=null");
                return;
            }

            if (intent == null)
            {
                this.logger.LogDebug($"ProcessIntent: intent=null");
                return;
            }

            var activityType = activity.GetType();
            var extras = intent.GetExtrasDict();
            this.logger.LogDebug($"ProcessIntent: activity.Type={activityType.Name}, intent.Flags={intent.Flags}, intent.Extras=[{string.Join(", ", extras)}]");

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
                    this.logger.LogDebug($"ProcessIntent: {intentAlreadyHandledKey} not present --> Process push notification");

                    // TODO: Refactor this! This is for sure not a good behavior..
                    DefaultNotificationActivityType = activityType;

                    var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;

                    if (extras.TryGetInt(DefaultPushNotificationHandler.ActionNotificationIdKey, out var notificationId))
                    {
                        if (extras.TryGetString(DefaultPushNotificationHandler.ActionNotificationTagKey, out var notificationTag))
                        {
                            notificationManager.Cancel(notificationId);
                        }
                        else
                        {
                            notificationManager.Cancel(notificationTag, notificationId);
                        }
                    }

                    // TODO: Pass object instead of 3 parameters
                    var identifier = extras.GetStringOrDefault(DefaultPushNotificationHandler.ActionIdentifierKey);
                    if (identifier == null)
                    {
                        this.HandleNotificationOpened(extras, identifier, NotificationCategoryType.Default);
                    }
                    else
                    {
                        this.HandleNotificationAction(extras, identifier, NotificationCategoryType.Default);
                    }
                }
                else
                {
                    this.logger.LogDebug($"ProcessIntent: {intentAlreadyHandledKey} is present --> Push notification already processed");
                }
            }
        }

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

            System.Diagnostics.Debug.WriteLine(CrossFirebasePushNotification.Current.Token);
        }

        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Initialize(Context context, NotificationUserCategory[] notificationCategories, bool resetToken, bool createDefaultNotificationChannel = true, bool autoRegistration = true)
        {
            this.Initialize(context, resetToken, createDefaultNotificationChannel, autoRegistration);
            this.RegisterUserNotificationCategories(notificationCategories);
        }

        public void Reset()
        {
            // TODO: QueueUserWorkItem is this really necessary resp. of any advantage here?
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    this.CleanUp();
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Reset failed with exception");
                    this.HandleNotificationError(FirebasePushNotificationErrorType.UnregistrationFailed, ex.ToString());
                }
            });
        }

        public async Task RegisterForPushNotificationsAsync()
        {
            this.logger.LogDebug("RegisterForPushNotificationsAsync");

            FirebaseMessaging.Instance.AutoInitEnabled = true;
            await Task.Run(async () =>
            {
                var token = await this.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    this.SaveToken(token);
                }
            });
        }

        private async Task<string> GetTokenAsync()
        {
            var tcs = new TaskCompletionSource<Java.Lang.Object>();
            var taskCompleteListener = new TaskCompleteListener(tcs);
            FirebaseMessaging.Instance.GetToken().AddOnCompleteListener(taskCompleteListener);

            string token = null;

            try
            {
                var taskResult = await tcs.Task;
                token = taskResult.ToString();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "GetTokenAsync failed with exception");
                this.HandleNotificationError(FirebasePushNotificationErrorType.RegistrationFailed, $"{ex}");
            }

            return token;
        }

        public void UnregisterForPushNotifications()
        {
            FirebaseMessaging.Instance.AutoInitEnabled = false;
            this.Reset();
        }

        private void CleanUp(bool clearAll = true)
        {
            if (clearAll)
            {
                this.UnsubscribeAll();
            }

            // TODO: DeleteToken seems to be an Android Task... await before continue!
            FirebaseMessaging.Instance.DeleteToken();

            this.SaveToken(string.Empty);
        }

        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Initialize(Context context, IPushNotificationHandler pushNotificationHandler, bool resetToken, bool createDefaultNotificationChannel = true, bool autoRegistration = true)
        {
            this.NotificationHandler = pushNotificationHandler;
            this.Initialize(context, resetToken, createDefaultNotificationChannel, autoRegistration);
        }

        public void ClearUserNotificationCategories()
        {
            this.userNotificationCategories.Clear();
        }

        // TODO: Read FirebaseTokenKey in a central place (now it's spread all over the code
        public string Token => Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).GetString(Constants.FirebaseTokenKey, string.Empty);

        public string[] SubscribedTopics
        {
            get
            {
                IList<string> topics = new List<string>();

                foreach (var t in this.currentTopics)
                {

                    topics.Add(t);
                }

                return topics.ToArray();
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

        public NotificationUserCategory[] GetUserNotificationCategories()
        {
            return this.userNotificationCategories?.ToArray();
        }

        public void RegisterUserNotificationCategories(NotificationUserCategory[] notificationCategories)
        {
            if (notificationCategories != null && notificationCategories.Length > 0)
            {
                this.ClearUserNotificationCategories();

                foreach (var userCat in notificationCategories)
                {
                    this.userNotificationCategories.Add(userCat);
                }

            }
            else
            {
                this.ClearUserNotificationCategories();
            }
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
            if (!this.currentTopics.Contains(topic))
            {
                FirebaseMessaging.Instance.SubscribeToTopic(topic);
                this.currentTopics.Add(topic);
                var editor = Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).Edit();
                editor.PutStringSet(Constants.FirebaseTopicsKey, this.currentTopics);
                editor.Commit();
            }
        }

        public void Unsubscribe(string[] topics)
        {
            foreach (var t in topics)
            {
                this.Unsubscribe(t);
            }
        }

        public void UnsubscribeAll()
        {
            foreach (var t in this.currentTopics)
            {
                if (this.currentTopics.Contains(t))
                {
                    FirebaseMessaging.Instance.UnsubscribeFromTopic(t);
                }
            }

            this.currentTopics.Clear();

            var editor = Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).Edit();
            editor.PutStringSet(Constants.FirebaseTopicsKey, this.currentTopics);
            editor.Commit();
        }

        public void Unsubscribe(string topic)
        {
            if (this.currentTopics.Contains(topic))
            {
                FirebaseMessaging.Instance.UnsubscribeFromTopic(topic);
                this.currentTopics.Remove(topic);

                var editor = Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).Edit();
                editor.PutStringSet(Constants.FirebaseTopicsKey, this.currentTopics);
                editor.Commit();
            }
        }

        protected override void OnTokenRefresh(string token)
        {
            this.SaveToken(token);
        }

        private void SaveToken(string token)
        {
            this.logger.LogDebug("SaveToken");

            using (var editor = Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).Edit())
            {
                editor.PutString(Constants.FirebaseTokenKey, token);
                editor.Commit();
            }
        }

        public void ClearAllNotifications()
        {
            var manager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            manager.CancelAll();
        }

        public void RemoveNotification(int id)
        {
            var manager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            manager.Cancel(id);
        }

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
