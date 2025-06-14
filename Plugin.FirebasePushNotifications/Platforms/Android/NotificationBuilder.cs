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
                    $"(Reason: Data key '{MessageNotificationKeys.NoUi}' is present)");
                return false;
            }

            if (notificationParams.Silent)
            {
                // If the message contains the silent key,
                // we don't display any local notification.
                this.logger.LogDebug(
                    $"ShouldHandleNotificationReceived returns false " +
                    $"(Reason: Data key '{Constants.SilentKey}' is present)");
                return false;
            }

            var isAppInForeground = AppHelper.IsAppForeground(Application.Context);
            var isAppInBackground = !isAppInForeground;

            if (!notificationParams.IsNotification)
            {
                this.logger.LogDebug(
                    $"ShouldHandleNotificationReceived returns false " +
                    $"(Reason: Data-only notification)");
                return false;
            }

            if (isAppInBackground)
            {
                this.logger.LogDebug(
                    $"ShouldHandleNotificationReceived returns true " +
                    $"(Reason: App runs in background mode)");
                return true;
            }

            var notificationImportance = GetNotificationImportance(data);
            if (notificationImportance >= NotificationImportance.High)
            {
                this.logger.LogDebug(
                    $"ShouldHandleNotificationReceived returns true " +
                    $"(Reason: Notification importance '{notificationImportance}' is higher than or equal to 'High')");
                return true;
            }

            // if (data.ContainsKey(Constants.LargeIconKey))
            // {
            //     // If we received a "large_icon"
            //     // we need to show a local notification with SetLargeIcon
            //     this.logger.LogDebug(
            //         $"ShouldHandleNotificationReceived returns true " +
            //         $"(Reason: Key '{Constants.LargeIconKey}' present)");
            //     return true;
            // }

            this.logger.LogDebug("ShouldHandleNotificationReceived returns false");
            return false;
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

            var notificationId = GetNotificationId(data);
            extras.PutInt(Constants.ActionNotificationIdKey, notificationId);

            var notificationTag = GetNotificationTag(data);
            if (notificationTag != null)
            {
                extras.PutString(Constants.ActionNotificationTagKey, notificationTag);
            }

            var context = Application.Context;
            var launchIntent = this.CreateActivityLaunchIntent(context);
            launchIntent.PutExtras(extras);

            if (this.options.Android.NotificationActivityFlags is ActivityFlags activityFlags)
            {
                launchIntent.SetFlags(activityFlags);
            }

            NotificationChannel notificationChannel;
            string notificationChannelId;
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                notificationChannel = null;
                notificationChannelId = NotificationChannel.DefaultChannelId;
            }
            else
            {
                notificationChannel = this.GetNotificationChannelOrDefault(data);
                if (notificationChannel == null)
                {
                    this.logger.LogError(
                        $"NotificationCompat.Builder requires a notification channel to work properly. " +
                        $"Use {nameof(INotificationChannels)}.{nameof(INotificationChannels.CreateNotificationChannels)} or " +
                        $"{nameof(INotificationChannels)}.{nameof(INotificationChannels.SetNotificationChannels)} " +
                        $"to create at least one notification channel.");
                    return;
                }

                notificationChannelId = notificationChannel.Id;
            }

            var notificationImportance = GetNotificationImportance(data);
            if (notificationChannel is { Importance: var notificationChannelImportance } &&
                notificationChannelImportance < notificationImportance)
            {
                this.logger.LogWarning(
                    $"Notification channel with Id={notificationChannelId} has Importance={notificationChannelImportance} " +
                    $"which is lower than '{notificationImportance}'.");
            }

            var smallIconResource = this.GetSmallIconResource(data, context);

            // TODO: Refactor this to avoid collisions!
            var requestCode = Rng.NextInt();

            var pendingIntent = PendingIntent.GetActivity(context, requestCode, launchIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var notificationBuilder = new NotificationCompat.Builder(context, notificationChannelId)
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
                // SetPriority was deprecated in API level 26.
                var notificationImportanceOrDefault = notificationImportance ?? this.options.Android.DefaultNotificationImportance;
                var notificationPriority = GetNotificationPriority(notificationImportanceOrDefault);
                notificationBuilder.SetPriority(notificationPriority);

                var notificationVibrationPattern = GetNotificationVibrationPattern(notificationImportanceOrDefault);
                if (notificationVibrationPattern != null)
                {
                    notificationBuilder.SetVibrate(notificationVibrationPattern);
                }

                try
                {
                    // SetSound was deprecated in API level 26.
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

                            var iconResource = this.GetIconResourceFromDrawableOrMipmap(context, notificationAction.Icon);
                            var action = new NotificationCompat.Action.Builder(iconResource, notificationAction.Title, pendingActionIntent).Build();
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

            if (notificationTag == null)
            {
                notificationManager.Notify(notificationId, notification);
            }
            else
            {
                notificationManager.Notify(notificationTag, notificationId, notification);
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
            var largeIconResource = this.GetIconResourceFromDrawableOrMipmap(context, data, Constants.LargeIconKey);

            if (largeIconResource == 0 &&
                data.TryGetString(Constants.LargeIconKey, out var largeIconUrl) &&
                System.Uri.IsWellFormedUriString(largeIconUrl, UriKind.Absolute))
            {
                return this.DownloadBitmap(largeIconUrl);
            }

            if (largeIconResource == 0 && this.options.Android.DefaultLargeIconResource is int defaultLargeIconResource)
            {
                largeIconResource = defaultLargeIconResource;
                var resourceName = this.TryGetResourceName(context, largeIconResource, nameof(largeIconResource), nameof(this.GetLargeIconBitmap), nameof(this.options.Android.DefaultLargeIconResource));
                if (resourceName == null)
                {
                    largeIconResource = 0;
                }
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
                this.logger.LogError(ex, "GetLargeIconBitmap: DecodeResource failed with exception");
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
            var smallIconResource = this.GetIconResourceFromDrawableOrMipmap(context, data, Constants.IconKey);

            if (smallIconResource == 0 && this.options.Android.DefaultIconResource is int defaultIconResource)
            {
                smallIconResource = defaultIconResource;
                if (smallIconResource != 0)
                {
                    var resourceName = this.TryGetResourceName(context, smallIconResource, nameof(smallIconResource), nameof(this.GetSmallIconResource), nameof(this.options.Android.DefaultIconResource));
                    if (resourceName == null)
                    {
                        smallIconResource = 0;
                    }
                }
            }

            if (smallIconResource == 0)
            {
                try
                {
                    var metadata = MetadataHelper.GetMetadata();
                    smallIconResource = metadata.GetInt(Constants.MetadataIconKey, 0);
                    if (smallIconResource != 0)
                    {
                        var resourceName = this.TryGetResourceName(context, smallIconResource, nameof(smallIconResource), nameof(this.GetSmallIconResource), "AndroidManifest.xml");
                        if (resourceName == null)
                        {
                            smallIconResource = 0;
                        }
                    }
                }
                catch (Exception)
                {
                    this.logger.LogDebug($"GetSmallIconResource: Failed to get {Constants.MetadataIconKey} from AndroidManifest.xml");
                }
            }

            if (smallIconResource == 0)
            {
                smallIconResource = context.ApplicationInfo.Icon;
                if (smallIconResource != 0)
                {
                    var resourceName = this.TryGetResourceName(context, smallIconResource, nameof(smallIconResource), nameof(this.GetSmallIconResource), "ApplicationInfo.Icon");
                    if (resourceName == null)
                    {
                        smallIconResource = 0;
                    }
                }
            }

            return smallIconResource;
        }

        private string TryGetResourceName(Context context, int resid, string residName, string methodName, string source)
        {
            string resourceName = null;

            try
            {
                if (resid != 0)
                {
                    resourceName = context.Resources.GetResourceName(resid);
                    if (resourceName != null)
                    {
                        this.logger.LogDebug($"{methodName}: {residName}={resid}, resourceName={resourceName} " +
                                             $"(Source: {source})");
                    }
                }
            }
            catch
            {
                // Ignore
            }

            return resourceName;
        }

        private int GetIconResourceFromDrawableOrMipmap(Context context, IDictionary<string, object> data, string iconKey)
        {
            var iconResource = 0;

            if (data.TryGetString(iconKey, out var iconName))
            {
                iconResource = this.GetIconResourceFromDrawableOrMipmap(context, iconName);
            }

            return iconResource;
        }

        private int GetIconResourceFromDrawableOrMipmap(Context context, string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                return 0;
            }

            var iconResource = this.GetIconResourceFromDrawable(context, iconName);

            if (iconResource == 0)
            {
                iconResource = this.GetIconResourceFromMipmap(context, iconName);
            }

            return iconResource;
        }

        private int GetIconResourceFromMipmap(Context context, string iconName)
        {
            return this.GetIconResource(context, iconName, "mipmap");
        }

        private int GetIconResourceFromDrawable(Context context, string iconName)
        {
            return this.GetIconResource(context, iconName, "drawable");
        }

        private int GetIconResource(Context context, string iconName, string defType)
        {
            var resourceId = context.Resources.GetIdentifier(iconName, defType, context.PackageName);
            var resourceName = this.TryGetResourceName(context, resourceId, nameof(resourceId), nameof(this.GetIconResource), defType);
            if (resourceName == null)
            {
                resourceId = 0;
            }

            return resourceId;
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
                    var metadata = MetadataHelper.GetMetadata();
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

        private static int GetNotificationId(IDictionary<string, object> data)
        {
            data.TryGetInt(Constants.IdKey, out var notificationId);
            return notificationId;
        }

        private static string GetNotificationTag(IDictionary<string, object> data)
        {
            data.TryGetString(Constants.NotificationTagKey, out var notificationTag);
            return notificationTag;
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

        private (NotificationImportance, string) GetNotificationImportanceOrDefault(IDictionary<string, object> data)
        {
            string notificationImportanceSource;

            if (GetNotificationImportance(data) is not NotificationImportance notificationImportance)
            {
                notificationImportance = this.options.Android.DefaultNotificationImportance;
                notificationImportanceSource = nameof(this.options.Android.DefaultNotificationImportance);
            }
            else
            {
                notificationImportanceSource = $"notification '{Constants.PriorityKey}' flag";
            }

            return (notificationImportance, notificationImportanceSource);
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
        private void ResolveLocalizedParameters(NotificationCompat.Builder notificationBuilder, IDictionary<string, object> data)
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

        private string GetLocalizedString(string name, string[] arguments, NotificationCompat.Builder notificationBuilder)
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