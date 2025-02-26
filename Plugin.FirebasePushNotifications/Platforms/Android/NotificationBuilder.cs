using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using Java.Util;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Extensions;
using Plugin.FirebasePushNotifications.Utils;
using Application = Android.App.Application;
using Color = Android.Graphics.Color;
using Uri = Android.Net.Uri;
using MessageNotificationKeys = Firebase.Messaging.Constants.MessageNotificationKeys;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public class NotificationBuilder : INotificationBuilder
    {
        private static readonly long[] DefaultVibrationPattern = new long[] { 1000, 1000, 1000, 1000, 1000 };
        private static readonly Java.Util.Random Rng = new Java.Util.Random();

        private readonly ILogger logger;
        private readonly INotificationChannels notificationChannels;
        private readonly FirebasePushNotificationOptions options;

        public NotificationBuilder(
            ILogger<NotificationBuilder> logger,
            INotificationChannels notificationChannels,
            FirebasePushNotificationOptions options)
        {
            this.logger = logger;
            this.notificationChannels = notificationChannels;
            this.options = options;
        }

        public virtual bool ShouldHandleNotificationReceived(IDictionary<string, object> data)
        {
            var notificationParams = new NotificationParams(data);

            if (notificationParams.NoUI)
            {
                // If the message contains the NoUI key,
                // we don't display any local notification.
                this.logger.LogDebug(
                    $"ShouldHandleNotificationReceived returns false " +
                    $"(Reason: Key '{MessageNotificationKeys.NoUi}' is present)");
                return false;
            }

            if (notificationParams.Silent)
            {
                // If the message contains the silent key,
                // we don't display any local notification.
                this.logger.LogDebug(
                    $"ShouldHandleNotificationReceived returns false " +
                    $"(Reason: Key '{Constants.SilentKey}' is present)");
                return false;
            }

            if (notificationParams.IsNotification)
            {
                var isAppInBackground = !AppHelper.IsAppForeground(Application.Context);
                if (isAppInBackground)
                {
                    return true;
                }
            }

            var notificationImportance = GetNotificationImportance(data);
            if (notificationImportance >= NotificationImportance.High)
            {
                // In case we receive a notification with priority >= high
                // we show it in a local notification popup.
                this.logger.LogDebug(
                    $"ShouldHandleNotificationReceived returns true " +
                    $"(Reason: Notification importance '{notificationImportance}' is greater or equal to 'high')");
                return true;
            }

            var defaultNotificationImportance = this.GetDefaultNotificationImportance();
            if (defaultNotificationImportance >= NotificationImportance.High)
            {
                // In case a default notification importance >= high is configured
                // we show it in a local notification popup.
                this.logger.LogDebug(
                    $"ShouldHandleNotificationReceived returns true " +
                    $"(Reason: Default notification importance '{defaultNotificationImportance}' is greater or equal to 'high')");
                return true;
            }

            var presentClickActionKeys = Constants.ClickActionKeys
                .Where(data.ContainsKey)
                .ToArray();

            if (presentClickActionKeys.Length > 0)
            {
                var isAppInBackground = !AppHelper.IsAppForeground(Application.Context);
                if (isAppInBackground)
                {
                    // If we received a "click_action" or "category"
                    // and we run in background mode
                    // we need to show a local notification with action buttons.
                    this.logger.LogDebug(
                        $"ShouldHandleNotificationReceived returns true " +
                        $"(Reason: {(presentClickActionKeys.Length == 1 ?
                            $"Key '{presentClickActionKeys.Single()}' is present" :
                            $"Keys [{string.Join(",", presentClickActionKeys)}] are present")})");
                    return true;
                }
            }

            var notificationChannel = this.GetNotificationChannel(data);
            if (notificationChannel is { Importance: >= NotificationImportance.High })
            {
                // In case we receive a notification which targets a specific notification channel
                // and the notification channel's importance is >= high
                // we show it in a local notification popup.
                this.logger.LogDebug(
                    $"ShouldHandleNotificationReceived returns true " +
                    $"(Reason: Target notification channel '{notificationChannel.Id}' " +
                    $"has importance '{notificationChannel.Importance}' greater or equal to 'high')");
                return true;
            }

            if (data.ContainsKey(Constants.LargeIconKey))
            {
                // If we received a "large_icon"
                // we need to show a local notification with SetLargeIcon
                this.logger.LogDebug(
                    $"ShouldHandleNotificationReceived returns true " +
                    $"(Reason: Key '{Constants.LargeIconKey}' present)");
                return true;
            }

            this.logger.LogDebug("ShouldHandleNotificationReceived returns false");
            return false;
        }

        void INotificationBuilder.OnNotificationReceived(IDictionary<string, object> data)
        {
            if (!this.ShouldHandleNotificationReceived(data))
            {
                return;
            }

            this.OnNotificationReceived(data);
        }

        /// <summary>
        /// This method is called if we have to build our own, custom notification using NotificationCompat.Builder.
        /// </summary>
        /// <param name="data">The notification payload.</param>
        /// <remarks>
        /// This method is only called if <see cref="ShouldHandleNotificationReceived"/>
        /// returns <c>true</c>.
        /// </remarks>
        public virtual void OnNotificationReceived(IDictionary<string, object> data)
        {
            this.logger.LogDebug("OnNotificationReceived");

            // TODO / WARNING:
            // Long term goal: A developer can use IPushNotificationHandler to intercept all notifications and do some operations on them.
            // All the logic in here should move to the Android-specific implementation of FirebasePushNotificationManager.

            var extras = new Bundle();
            foreach (var kvp in data)
            {
                extras.PutString(kvp.Key, kvp.Value.ToString());
            }

            var notificationId = this.GetNotificationId(data);
            extras.PutInt(Constants.ActionNotificationIdKey, notificationId);

            if (data.TryGetString(Constants.NotificationTagKey, out var tag))
            {
                extras.PutString(Constants.ActionNotificationTagKey, tag);
            }

            var context = Application.Context;
            var launchIntent = this.CreateActivityLaunchIntent(context);
            launchIntent.PutExtras(extras);

            if (this.options.Android.NotificationActivityFlags is ActivityFlags activityFlags)
            {
                launchIntent.SetFlags(activityFlags);
            }

            var notificationChannel = this.GetNotificationChannelOrDefault(data);
            if (notificationChannel == null)
            {
                this.logger.LogError(
                    $"NotificationCompat.Builder requires a notification channel to work properly. " +
                    $"Use {nameof(INotificationChannels)}.{nameof(INotificationChannels.CreateNotificationChannels)} " +
                    $"to create at least one notification channel.");
                return;
            }

            var notificationImportance = this.GetNotificationImportanceOrDefault(data);
            if (notificationChannel.Importance < notificationImportance)
            {
                this.logger.LogWarning(
                    $"Notification channel '{notificationChannel.Id}' has importance '{notificationChannel.Importance}' " +
                    $"which is lower than notification importance '{notificationImportance}'");
            }

            var smallIconResource = this.GetSmallIconResource(data, context);

            // TODO: Refactor this to avoid collisions!
            var requestCode = Rng.NextInt();

            var pendingIntent = PendingIntent.GetActivity(context, requestCode, launchIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var notificationBuilder = new NotificationCompat.Builder(context, notificationChannel.Id)
                .SetSmallIcon(smallIconResource)
                .SetAutoCancel(true)
                .SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis())
                .SetContentIntent(pendingIntent);

            var messageTitle = this.GetNotificationTitle(data) ?? GetDefaultNotificationTitle(context);
            if (!string.IsNullOrEmpty(messageTitle))
            {
                notificationBuilder.SetContentTitle(messageTitle);
            }

            var messageBody = this.GetNotificationBody(data);
            if (!string.IsNullOrEmpty(messageBody))
            {
                notificationBuilder.SetContentText(messageBody);
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
            {
                var showWhenVisible = this.GetShowWhenVisible(data);
                notificationBuilder.SetShowWhen(showWhenVisible);
            }

            var largeIconBitmap = this.GetLargeIconBitmap(data, context);
            if (largeIconBitmap != null)
            {
                notificationBuilder.SetLargeIcon(largeIconBitmap);

                notificationBuilder.SetStyle(new NotificationCompat.BigPictureStyle()
                    .BigPicture(largeIconBitmap)
                    .BigLargeIcon((Bitmap)null));
            }

            var deleteIntent = new Intent(context, typeof(PushNotificationDeletedReceiver));
            deleteIntent.PutExtras(extras);
            var pendingDeleteIntent = PendingIntent.GetBroadcast(context, requestCode, deleteIntent,
                PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable);
            notificationBuilder.SetDeleteIntent(pendingDeleteIntent);

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                var notificationPriority = GetNotificationPriority(notificationImportance);
                notificationBuilder.SetPriority(notificationPriority);

                var notificationVibrationPattern = GetNotificationVibrationPattern(notificationImportance);
                if (notificationVibrationPattern != null)
                {
                    notificationBuilder.SetVibrate(notificationVibrationPattern);
                }

                try
                {
                    var soundUri = this.GetSoundUri(data, context);
                    notificationBuilder.SetSound(soundUri);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "SetSound failed with exception");
                }
            }

            // Try to resolve (and apply) localized parameters
            this.ResolveLocalizedParameters(notificationBuilder, data);

            var notificationColor = this.GetNotificationColor(data);
            if (notificationColor != null)
            {
                notificationBuilder.SetColor(notificationColor.Value);
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
            {
                var notificationStyle = this.GetNotificationStyle(data, messageBody);
                if (notificationStyle != null)
                {
                    notificationBuilder.SetStyle(notificationStyle);
                }
            }

            var category = GetCategoryValue(data);

            if (!string.IsNullOrEmpty(category))
            {
                var allNotificationCategories = IFirebasePushNotification.Current.NotificationCategories;
                if (allNotificationCategories is { Length: > 0 })
                {
                    var notificationCategory = allNotificationCategories.SingleOrDefault(c =>
                        string.Equals(c.CategoryId, category, StringComparison.InvariantCultureIgnoreCase));

                    if (notificationCategory != null)
                    {
                        foreach (var notificationAction in notificationCategory.Actions)
                        {
                            extras.PutString(Constants.NotificationCategoryKey, notificationCategory.CategoryId);
                            extras.PutString(Constants.NotificationActionId, notificationAction.Id);

                            var aRequestCode = Guid.NewGuid().GetHashCode();

                            Intent actionIntent;
                            PendingIntent pendingActionIntent;
                            if (notificationAction.Type == NotificationActionType.Foreground)
                            {
                                actionIntent = this.CreateActivityLaunchIntent(context);
                                actionIntent.PutExtras(extras);

                                if (this.options.Android.NotificationActivityFlags is ActivityFlags intentActivityFlags)
                                {
                                    actionIntent.SetFlags(intentActivityFlags);
                                }

                                pendingActionIntent = PendingIntent.GetActivity(context, aRequestCode, actionIntent,
                                    PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
                            }
                            else
                            {
                                actionIntent = new Intent(context, typeof(PushNotificationActionReceiver));
                                actionIntent.PutExtras(extras);
                                pendingActionIntent = PendingIntent.GetBroadcast(context, aRequestCode, actionIntent,
                                    PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
                            }

                            var icon = context.Resources.GetIdentifier(notificationAction.Icon ?? "", "drawable", context.PackageName);
                            var action = new NotificationCompat.Action.Builder(icon, notificationAction.Title, pendingActionIntent).Build();
                            notificationBuilder.AddAction(action);
                        }
                    }
                    else
                    {
                        this.logger.LogWarning(
                            $"Category '{category}' not found in the list of registered notification categories " +
                            $"(see {nameof(IFirebasePushNotification)}.{nameof(IFirebasePushNotification.NotificationCategories)})");
                    }
                }
                else
                {
                    this.logger.LogWarning(
                        $"Category '{category}' is present in notification data list of registered notification categories is empty " +
                        $"(see {nameof(IFirebasePushNotification)}.{nameof(IFirebasePushNotification.NotificationCategories)})");
                }
            }

            this.OnBuildNotification(notificationBuilder, data);

            var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            var notification = notificationBuilder.Build();

            if (tag == null)
            {
                notificationManager.Notify(notificationId, notification);
            }
            else
            {
                notificationManager.Notify(tag, notificationId, notification);
            }
        }

        private NotificationCompat.Style GetNotificationStyle(IDictionary<string, object> data, string messageBody)
        {
            bool useBigTextStyle;

            if (data.TryGetBool(Constants.BigTextStyleKey, out var shouldUseBigTextStyle))
            {
                useBigTextStyle = shouldUseBigTextStyle;
            }
            else
            {
                useBigTextStyle = this.options.Android.UseBigTextStyle;
            }

            NotificationCompat.Style style = null;

            if (useBigTextStyle)
            {
                var bigTextStyle = new NotificationCompat.BigTextStyle();
                bigTextStyle.BigText(messageBody);
                style = bigTextStyle;
            }

            // Add more styles, if needed...

            return style;
        }

        private Uri GetSoundUri(IDictionary<string, object> data, Context context)
        {
            var soundUri = this.options.Android.SoundUri;

            try
            {
                if (data.TryGetString(Constants.SoundKey, out var soundName))
                {
                    var soundResId = context.Resources.GetIdentifier(soundName, "raw", context.PackageName);
                    if (soundResId == 0 && soundName.IndexOf(".") != -1)
                    {
                        soundName = soundName[..soundName.LastIndexOf('.')];
                        soundResId = context.Resources.GetIdentifier(soundName, "raw", context.PackageName);
                    }

                    soundUri = new Uri.Builder()
                        .Scheme(ContentResolver.SchemeAndroidResource)
                        .Path($"{context.PackageName}/{soundResId}")
                        .Build();
                }
            }
            catch (Resources.NotFoundException ex)
            {
                this.logger.LogError(ex, "Failed to get sound");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get sound");
            }

            soundUri ??= RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            return soundUri;
        }

        private static int GetNotificationPriority(NotificationImportance notificationImportance)
        {
            switch (notificationImportance)
            {
                case NotificationImportance.Max:
                    return NotificationCompat.PriorityMax;
                case NotificationImportance.High:
                    return NotificationCompat.PriorityHigh;
                case NotificationImportance.Default:
                    return NotificationCompat.PriorityDefault;
                case NotificationImportance.Low:
                    return NotificationCompat.PriorityLow;
                case NotificationImportance.Min:
                    return NotificationCompat.PriorityMin;
                default:
                    return NotificationCompat.PriorityDefault;
            }
        }

        private static long[] GetNotificationVibrationPattern(NotificationImportance notificationImportance)
        {
            switch (notificationImportance)
            {
                case NotificationImportance.Unspecified:
                case NotificationImportance.None:
                case NotificationImportance.Max:
                case NotificationImportance.High:
                case NotificationImportance.Default:
                    return DefaultVibrationPattern;
                case NotificationImportance.Low:
                case NotificationImportance.Min:
                    return null;
                default:
                    return DefaultVibrationPattern;
            }
        }

        private Bitmap GetLargeIconBitmap(IDictionary<string, object> data, Context context)
        {
            var largeIconResource = GetIconResourceFromDrawableOrMipmap(context, data, Constants.LargeIconKey);

            if (largeIconResource == 0 &&
                data.TryGetString(Constants.LargeIconKey, out var largeIconUrl) &&
                System.Uri.IsWellFormedUriString(largeIconUrl, UriKind.Absolute))
            {
                return this.DownloadBitmap(largeIconUrl);
            }

            if (largeIconResource == 0 && this.options.Android.DefaultLargeIconResource is int defaultLargeIconResource)
            {
                largeIconResource = defaultLargeIconResource;
            }

            try
            {
                var name = context.Resources.GetResourceName(largeIconResource);
                if (name == null)
                {
                    largeIconResource = 0;
                }
            }
            catch (Resources.NotFoundException ex)
            {
                this.logger.LogError(ex, "Failed to get large icon resource");
                largeIconResource = 0;
            }

            Bitmap largeIconBitmap = null;

            try
            {
                if (largeIconResource != 0)
                {
                    largeIconBitmap = BitmapFactory.DecodeResource(context.Resources, largeIconResource);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to decode bitmap ");
            }

            return largeIconBitmap;
        }

        private Bitmap DownloadBitmap(string url)
        {
            Bitmap bitmap = null;

            try
            {
                using (var connection = new Java.Net.URL(url).OpenConnection())
                {
                    if (connection == null)
                    {
                        return null;
                    }

                    connection.DoInput = true;
                    connection.Connect();
                    bitmap = BitmapFactory.DecodeStream(connection.InputStream);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "DownloadBitmap failed with exception");
            }

            return bitmap;
        }

        private int GetSmallIconResource(IDictionary<string, object> data, Context context)
        {
            var smallIconResource = GetIconResourceFromDrawableOrMipmap(context, data, Constants.IconKey);

            if (smallIconResource == 0 && this.options.Android.DefaultIconResource is int defaultIconResource)
            {
                smallIconResource = defaultIconResource;
            }

            if (smallIconResource == 0)
            {
                try
                {
                    var metadata = GetMetadata();
                    smallIconResource = metadata.GetInt(Constants.MetadataIconKey, 0);
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning(ex, "Failed to get default notification icon from meta-data");
                }
            }

            try
            {
                var name = context.Resources.GetResourceName(smallIconResource);
                if (name == null)
                {
                    smallIconResource = 0;
                }
            }
            catch (Resources.NotFoundException ex)
            {
                this.logger.LogError(ex, "Failed to get small icon resource");
                smallIconResource = 0;
            }

            if (smallIconResource == 0)
            {
                smallIconResource = context.ApplicationInfo.Icon;
            }

            return smallIconResource;
        }

        private static int GetIconResourceFromDrawableOrMipmap(Context context, IDictionary<string, object> data, string dataKey)
        {
            var largeIconResource = 0;

            if (data.TryGetString(dataKey, out var largeIcon) && largeIcon != null)
            {
                largeIconResource = context.Resources.GetIdentifier(largeIcon, "drawable", context.PackageName);
                if (largeIconResource == 0)
                {
                    largeIconResource = context.Resources.GetIdentifier(largeIcon, "mipmap", context.PackageName);
                }
            }

            return largeIconResource;
        }

        private int? GetNotificationColor(IDictionary<string, object> data)
        {
            int? notificationColor = null;

            if (data.TryGetString(Constants.ColorKey, out var colorValue) && colorValue != null)
            {
                try
                {
                    notificationColor = Color.ParseColor(colorValue);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to parse color");
                }
            }

            if (notificationColor == null && this.options.Android.DefaultColor is Color defaultColor)
            {
                notificationColor = defaultColor;
            }

            if (notificationColor == null)
            {
                try
                {
                    var metadata = GetMetadata();
                    var resourceId = metadata.GetInt(Constants.MetadataColorKey, 0);
                    if (resourceId != 0)
                    {
                        notificationColor = AndroidX.Core.Content.ContextCompat.GetColor(Application.Context, resourceId);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to get default notification color from meta-data");
                }
            }

            return notificationColor;
        }

        private static Bundle GetMetadata()
        {
            var applicationInfo = Application.Context.PackageManager.GetApplicationInfo(
                Application.Context.PackageName,
                Android.Content.PM.PackageInfoFlags.MetaData);

            var metadata = applicationInfo.MetaData;

            return metadata;
        }

        private Intent CreateActivityLaunchIntent(Context context)
        {
            Intent launchIntent;

            if (this.options.Android.NotificationActivityType is Type notificationActivityType)
            {
                launchIntent = new Intent(context, notificationActivityType);
            }
            else
            {
                launchIntent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);
            }

            return launchIntent;
        }

        private bool GetShowWhenVisible(IDictionary<string, object> data)
        {
            bool showWhenVisible;
            if (data.TryGetBool(Constants.ShowWhenKey, out var shouldShowWhen))
            {
                showWhenVisible = shouldShowWhen;
            }
            else
            {
                showWhenVisible = this.options.Android.ShouldShowWhen;
            }

            return showWhenVisible;
        }

        private NotificationChannel GetNotificationChannelOrDefault(IDictionary<string, object> data)
        {
            var notificationChannel = this.GetNotificationChannel(data);

            if (notificationChannel == null)
            {
                notificationChannel = this.notificationChannels.Channels.GetDefault();
            }

            // if (notificationChannel == null)
            // {
            //     // TODO: Read default notification channel from manifest
            //     // Source: https://github.com/firebase/firebase-android-sdk/blob/1e8c2185411d6b62e8a6a74de91d4dccf40838c7/firebase-messaging/src/main/java/com/google/firebase/messaging/CommonNotificationBuilder.java#L62
            //     //  public static final String METADATA_DEFAULT_CHANNEL_ID =
            //     //      "com.google.firebase.messaging.default_notification_channel_id";
            // }

            return notificationChannel;
        }

        private NotificationChannel GetNotificationChannel(IDictionary<string, object> data)
        {
            NotificationChannel notificationChannel = null;

            if (data.TryGetString(Constants.ChannelIdKey, out var channelId))
            {
                notificationChannel = this.notificationChannels.Channels.GetById(channelId);
            }

            return notificationChannel;
        }

        private int GetNotificationId(IDictionary<string, object> data)
        {
            var notificationId = 0;

            // TODO: Use TryGetInt here
            if (data.TryGetString(Constants.IdKey, out var id))
            {
                try
                {
                    notificationId = Convert.ToInt32(id);
                }
                catch (Exception ex)
                {
                    // Keep the default value of zero for the notify_id, but log the conversion problem.
                    this.logger.LogError(ex, $"Failed to convert {id} to an integer");
                }
            }

            return notificationId;
        }

        private static NotificationImportance? GetNotificationImportance(IDictionary<string, object> data)
        {
            NotificationImportance? notificationImportance = null;

            if (data.TryGetString(Constants.PriorityKey, out var priorityValue))
            {
                notificationImportance = GetNotificationImportance(priorityValue);
            }

            return notificationImportance;
        }

        private NotificationImportance GetNotificationImportanceOrDefault(IDictionary<string, object> data)
        {
            if (GetNotificationImportance(data) is not NotificationImportance notificationImportance)
            {
                notificationImportance = this.GetDefaultNotificationImportance();
            }

            return notificationImportance;
        }

        private static NotificationImportance GetNotificationImportance(string priorityValue)
        {
            switch (priorityValue?.ToLowerInvariant())
            {
                case "unspecified":
                    return NotificationImportance.Unspecified;
                case "none":
                    return NotificationImportance.None;
                case "min":
                    return NotificationImportance.Min;
                case "low":
                    return NotificationImportance.Low;
                case "default":
                    return NotificationImportance.Default;
                case "high":
                    return NotificationImportance.High;
                case "max":
                    return NotificationImportance.Max;
                default:
                    return NotificationImportance.Default;
            }
        }

        private NotificationImportance GetDefaultNotificationImportance()
        {
            return this.options.Android.DefaultNotificationImportance;
        }

        private string GetNotificationBody(IDictionary<string, object> data)
        {
            string messageBody;

            if (this.options.Android.NotificationBodyKey is string notificationBodyKey &&
                data.TryGetString(notificationBodyKey, out var notificationBodyValue))
            {
                messageBody = notificationBodyValue;
            }
            else if (data.TryGetString(Constants.AlertKey, out var alert))
            {
                messageBody = alert;
            }
            else if (data.TryGetString(Constants.NotificationBodyKey, out var body))
            {
                messageBody = body;
            }
            else if (data.TryGetString(Constants.GcmNotificationBodyKey, out var notificationBody))
            {
                messageBody = notificationBody;
            }
            else if (data.TryGetString(Constants.MessageKey, out var messageContent))
            {
                messageBody = messageContent;
            }
            else if (data.TryGetString(Constants.SubtitleKey, out var subtitle))
            {
                messageBody = subtitle;
            }
            else if (data.TryGetString(Constants.TextKey, out var text))
            {
                messageBody = text;
            }
            else
            {
                messageBody = null;
            }

            return messageBody;
        }

        private string GetNotificationTitle(IDictionary<string, object> data)
        {
            string messageTitle;

            if (this.options.Android.NotificationTitleKey is string notificationTitleKey &&
                data.TryGetString(notificationTitleKey, out var notificationTitleValue))
            {
                messageTitle = notificationTitleValue;
            }
            else if (data.TryGetString(Constants.NotificationTitleKey, out var titleContent))
            {
                messageTitle = titleContent;
            }
            else if (data.TryGetString(Constants.GcmNotificationTitleKey, out var notificationTitle))
            {
                messageTitle = notificationTitle;
            }
            else
            {
                messageTitle = null;
            }

            return messageTitle;
        }

        private static string GetDefaultNotificationTitle(Context context)
        {
            return context.ApplicationInfo.LoadLabel(context.PackageManager);
        }

        private static string GetCategoryValue(IDictionary<string, object> data)
        {
            string category = null;

            foreach (var clickActionKey in Constants.ClickActionKeys)
            {
                if (data.TryGetString(clickActionKey, out var value))
                {
                    category = value;
                    break;
                }
            }

            return category;
        }

        /// <summary>
        /// Resolves the localized parameters using the string resources, combining the key and the passed arguments of the notification.
        /// </summary>
        /// <param name="notificationBuilder">Notification builder.</param>
        /// <param name="data">Data payload.</param>
        private void ResolveLocalizedParameters(NotificationCompat.Builder notificationBuilder,
            IDictionary<string, object> data)
        {
            // Resolve title localization
            if (data.TryGetString("title_loc_key", out var titleKey))
            {
                data.TryGetValue("title_loc_args", out var titleArgs);

                var localizedTitle = this.GetLocalizedString(titleKey, titleArgs as string[], notificationBuilder);
                if (localizedTitle != null)
                {
                    notificationBuilder.SetContentTitle(localizedTitle);
                }
            }

            // Resolve body localization
            if (data.TryGetString("body_loc_key", out var bodyKey))
            {
                data.TryGetValue("body_loc_args", out var bodyArgs);

                var localizedBody = this.GetLocalizedString(bodyKey, bodyArgs as string[], notificationBuilder);
                if (localizedBody != null)
                {
                    notificationBuilder.SetContentText(localizedBody);
                }
            }
        }

        private string GetLocalizedString(string name, string[] arguments,
            NotificationCompat.Builder notificationBuilder)
        {
            try
            {
                var context = notificationBuilder.MContext;
                var resources = context.Resources;
                var identifier = resources.GetIdentifier(name, "string", context.PackageName);
                var sanitizedArgs = arguments?.Where(it => it != null)
                    .Select(it => new Java.Lang.String(it))
                    .Cast<Java.Lang.Object>()
                    .ToArray() ?? Array.Empty<Java.Lang.Object>();

                return resources.GetString(identifier, sanitizedArgs);
            }
            catch (UnknownFormatConversionException ex)
            {
                this.logger.LogError(ex, "GetLocalizedString - Incorrect string arguments");
                return null;
            }
        }

        /// <summary>
        /// Override to provide customization of the notification to build.
        /// </summary>
        /// <param name="notificationBuilder">Notification builder.</param>
        /// <param name="data">Notification data.</param>
        public virtual void OnBuildNotification(NotificationCompat.Builder notificationBuilder, IDictionary<string, object> data)
        {
        }
    }
}