using System.Collections.ObjectModel;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Firebase.Messaging;
using Microsoft.Extensions.Logging;
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

        public override void Configure(FirebasePushNotificationOptions options)
        {
            base.Configure(options);

            NotificationActivityType = options.Android.NotificationActivityType;
            DefaultNotificationChannelId = options.Android.DefaultNotificationChannelId;
        }

        //internal static PushNotificationActionReceiver ActionReceiver = new PushNotificationActionReceiver();

        public void ProcessIntent(Activity activity, Intent intent)
        {
            if (intent == null)
            {
                return;
            }

            var extras = intent.GetExtras();
            this.logger.LogDebug($"ProcessIntent: Flags={intent.Flags}, Extras={string.Join(System.Environment.NewLine, extras)}");

            var launchedFromHistory = intent.Flags.HasFlag(ActivityFlags.LaunchedFromHistory);
            if (launchedFromHistory)
            {
                // Don't process the intent if flag FLAG_ACTIVITY_LAUNCHED_FROM_HISTORY is present
                return;
            }

            if (intent.Extras != null &&
                intent.Extras.KeySet().Any())
            {
                // Don't process old/historic intents which are recycled for whatever reasons
                var intentAlreadyHandledKey = Constants.ExtraFirebaseProcessIntentHandled;
                if (!intent.GetBooleanExtra(intentAlreadyHandledKey, false))
                {
                    intent.PutExtra(intentAlreadyHandledKey, true);
                    this.logger.LogDebug($"ProcessIntent: {intentAlreadyHandledKey} not present --> Process push notification");
                    this.ProcessIntentOld(activity, intent);
                }
                else
                {
                    this.logger.LogDebug($"ProcessIntent: {intentAlreadyHandledKey} is present --> Push notification already processed");
                }
            }
        }

        // TODO: Merge with ProcessIntent method to avoid duplicated code & checks
        private void ProcessIntentOld(Activity activity, Intent intent)
        {
            DefaultNotificationActivityType = activity.GetType();
            var extras = intent?.Extras;
            if (extras != null && !extras.IsEmpty)
            {
                var parameters = new Dictionary<string, object>();
                foreach (var key in extras.KeySet())
                {
                    if (!parameters.ContainsKey(key) && extras.Get(key) != null)
                    {
                        parameters.Add(key, $"{extras.Get(key)}");
                    }
                }

                if (parameters.Count > 0)
                {
                    var manager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
                    var notificationId = extras.GetInt(DefaultPushNotificationHandler.ActionNotificationIdKey, -1);
                    if (notificationId != -1)
                    {
                        var notificationTag = extras.GetString(DefaultPushNotificationHandler.ActionNotificationTagKey, string.Empty);
                        if (notificationTag == null)
                        {
                            manager.Cancel(notificationId);
                        }
                        else
                        {
                            manager.Cancel(notificationTag, notificationId);
                        }
                    }

                    var response = new NotificationResponse(parameters, extras.GetString(DefaultPushNotificationHandler.ActionIdentifierKey, string.Empty));


                    if (string.IsNullOrEmpty(response.Identifier))
                    {
                        this.onNotificationOpened?.Invoke(CrossFirebasePushNotification.Current, new FirebasePushNotificationResponseEventArgs(response.Data, response.Identifier, response.Type));
                    }
                    else
                    {
                        this.onNotificationAction?.Invoke(CrossFirebasePushNotification.Current, new FirebasePushNotificationResponseEventArgs(response.Data, response.Identifier, response.Type));
                    }

                    this.NotificationHandler?.OnOpened(response);
                }
            }
        }

        [Obsolete]
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
                        this.onNotificationError?.Invoke(CrossFirebasePushNotification.Current, new FirebasePushNotificationErrorEventArgs(FirebasePushNotificationErrorType.UnregistrationFailed, ex.ToString()));
                    }
                    finally
                    {
                        var editor = prefs.Edit();
                        editor.PutString(AppVersionNameKey, $"{versionName}");
                        editor.PutString(AppVersionCodeKey, $"{versionCode}");
                        editor.PutString(AppVersionPackageNameKey, $"{packageName}");
                        editor.Commit();
                    }

                    CrossFirebasePushNotification.Current.RegisterForPushNotifications();
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
        public void Initialize(Context context, NotificationUserCategory[] notificationCategories, bool resetToken, bool createDefaultNotificationChannel = true, bool autoRegistration = true)
        {
            this.Initialize(context, resetToken, createDefaultNotificationChannel, autoRegistration);
            this.RegisterUserNotificationCategories(notificationCategories);
        }

        public void Reset()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    this.CleanUp();
                }
                catch (Exception ex)
                {
                    this.onNotificationError?.Invoke(CrossFirebasePushNotification.Current, new FirebasePushNotificationErrorEventArgs(FirebasePushNotificationErrorType.UnregistrationFailed, ex.ToString()));
                }
            });
        }

        public void RegisterForPushNotifications()
        {
            FirebaseMessaging.Instance.AutoInitEnabled = true;
            Task.Run(async () =>
            {
                var token = await this.GetTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {

                    SaveToken(token);
                }
            });
        }

        public async Task<string> GetTokenAsync()
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
                this.onNotificationError?.Invoke(CrossFirebasePushNotification.Current, new FirebasePushNotificationErrorEventArgs(FirebasePushNotificationErrorType.RegistrationFailed, $"{ex}"));
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

            FirebaseMessaging.Instance.DeleteToken();
            SaveToken(string.Empty);
        }

        [Obsolete]
        public void Initialize(Context context, IPushNotificationHandler pushNotificationHandler, bool resetToken, bool createDefaultNotificationChannel = true, bool autoRegistration = true)
        {
            this.NotificationHandler = pushNotificationHandler;
            this.Initialize(context, resetToken, createDefaultNotificationChannel, autoRegistration);
        }

        public void ClearUserNotificationCategories()
        {
            this.userNotificationCategories.Clear();
        }

        public string Token => Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).GetString(Constants.FirebaseTokenKey, string.Empty);

        public IPushNotificationHandler NotificationHandler { get; set; }

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

        //private static FirebasePushNotificationResponseEventHandler onNotificationOpened;
        //public event FirebasePushNotificationResponseEventHandler OnNotificationOpened
        //{
        //    add
        //    {
        //        var previousVal = onNotificationOpened;
        //        _onNotificationOpened += value;
        //        if (delayedNotificationResponse != null && previousVal == null)
        //        {
        //            var tmpParams = delayedNotificationResponse;
        //            if (string.IsNullOrEmpty(tmpParams.Identifier))
        //            {
        //                onNotificationOpened?.Invoke(this, new FirebasePushNotificationResponseEventArgs(tmpParams.Data, tmpParams.Identifier, tmpParams.Type));
        //                delayedNotificationResponse = null;
        //            }

        //        }

        //    }
        //    remove
        //    {
        //        _onNotificationOpened -= value;
        //    }
        //}

        //private FirebasePushNotificationResponseEventHandler _onNotificationAction;
        //public event FirebasePushNotificationResponseEventHandler OnNotificationAction
        //{
        //    add
        //    {
        //        var previousVal = _onNotificationAction;
        //        _onNotificationAction += value;
        //        if (delayedNotificationResponse != null && previousVal == null)
        //        {
        //            var tmpParams = delayedNotificationResponse;
        //            if (!string.IsNullOrEmpty(tmpParams.Identifier))
        //            {
        //                _onNotificationAction?.Invoke(CrossFirebasePushNotification.Current, new FirebasePushNotificationResponseEventArgs(tmpParams.Data, tmpParams.Identifier, tmpParams.Type));
        //                delayedNotificationResponse = null;
        //            }

        //        }
        //    }
        //    remove
        //    {
        //        _onNotificationAction -= value;
        //    }
        //}


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

        public void RegisterToken(string token)
        {
            SaveToken(token);
            this.onTokenRefresh?.Invoke(CrossFirebasePushNotification.Current, new FirebasePushNotificationTokenEventArgs(token));
        }

        public void RegisterData(IDictionary<string, object> data)
        {
            this.NotificationReceivedEventHandler.Invoke(CrossFirebasePushNotification.Current, new FirebasePushNotificationDataEventArgs(data));
        }

        public void RegisterAction(IDictionary<string, object> data)
        {
            // TODO: Inefficient code; refactoring required!
            var response = new NotificationResponse(data, data.ContainsKey(DefaultPushNotificationHandler.ActionIdentifierKey) ? $"{data[DefaultPushNotificationHandler.ActionIdentifierKey]}" : string.Empty);

            this.onNotificationAction?.Invoke(CrossFirebasePushNotification.Current, new FirebasePushNotificationResponseEventArgs(response.Data, response.Identifier, response.Type));
        }

        public void RegisterDelete(IDictionary<string, object> data)
        {
            this.onNotificationDeleted?.Invoke(CrossFirebasePushNotification.Current, new FirebasePushNotificationDataEventArgs(data));
        }

        private static void SaveToken(string token)
        {
            var editor = Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).Edit();
            editor.PutString(Constants.FirebaseTokenKey, token);
            editor.Commit();
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
