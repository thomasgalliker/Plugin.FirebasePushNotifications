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
using Plugin.FirebasePushNotifications.Platforms.Channels;
using Application = Android.App.Application;
using Color = Android.Graphics.Color;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public class NotificationBuilder : INotificationBuilder
    {
        private static readonly long[] DefaultVibrationPattern = new long[] { 1000, 1000, 1000, 1000, 1000 };
        private static readonly Java.Util.Random RNG = new Java.Util.Random();

        private readonly ILogger logger;
        private readonly FirebasePushNotificationOptions options;

        public NotificationBuilder(
            ILogger<NotificationBuilder> logger,
            FirebasePushNotificationOptions options)
        {
            this.logger = logger;
            this.options = options;
        }

        public virtual bool ShouldHandleNotificationReceived(IDictionary<string, object> data)
        {
            if (data.TryGetBool(Constants.SilentKey, out var silentValue) && silentValue)
            {
                // If the message contains the silent key,
                // we don't display any local notification.
                return false;
            }

            if (data.ContainsKey(Constants.ClickActionKey) || data.ContainsKey(Constants.CategoryKey))
            {
                // If we received a "click_action" or "category"
                // we need to create and show a local notification with action buttons.
                return true;
            }

            var notificationImportance = this.GetNotificationImportance(data);
            var isInForeground = IsInForeground();
            if (isInForeground == false)
            {
                if (notificationImportance >= NotificationImportance.High)
                {
                    // In case we receive a notification with priority >= high
                    // while the app runs in background mode,
                    // we show it in a local notification popup.
                    return true;
                }

                var notificationChannel = GetChannel(data);
                if (notificationChannel is { Importance: >= NotificationImportance.High })
                {
                    // In case we receive a notification which targets a specific notification channel
                    // and the notification channel's importance is >= high
                    // while the app runs in background mode,
                    // we show it in a local notification popup.
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        void INotificationBuilder.OnNotificationReceived(IDictionary<string, object> data)
        {
            if (!this.ShouldHandleNotificationReceived(data))
            {
                return;
            }

            this.OnNotificationReceived(data);
        }

        public virtual void OnNotificationReceived(IDictionary<string, object> data)
        {
            this.logger.LogDebug("OnNotificationReceived");

            // TODO / WARNING:
            // Long term goal: A developer can use IPushNotificationHandler to intercept all notifications and do some operations on them.
            // All the logic in here should move to the Android-specific implementation of FirebasePushNotificationManager.

            var context = Application.Context;

            // TODO: Cleanup these variables. There is a lot of legacy code from the Xamarin plugin here.
            var useBigTextStyle = FirebasePushNotificationManager.UseBigTextStyle;
            var soundUri = FirebasePushNotificationManager.SoundUri;

            if (data.TryGetBool(Constants.BigTextStyleKey, out var shouldUseBigTextStyle))
            {
                useBigTextStyle = shouldUseBigTextStyle;
            }

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

                    soundUri = new Android.Net.Uri.Builder()
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

            var resultIntent = CreateActivityLaunchIntent(context);

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

            resultIntent.PutExtras(extras);

            if (FirebasePushNotificationManager.NotificationActivityFlags is ActivityFlags activityFlags)
            {
                resultIntent.SetFlags(activityFlags);
            }

            // TODO: Refactor this to avoid collisions!
            var requestCode = RNG.NextInt();

            var pendingIntent = PendingIntent.GetActivity(context, requestCode, resultIntent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var notificationImportance = this.GetNotificationImportance(data);

            var notificationChannel = GetChannelOrDefault(data);
            if (notificationChannel.Importance < notificationImportance)
            {
                this.logger.LogWarning(
                    $"Notification channel '{notificationChannel.ChannelId}' has Importance={notificationChannel.Importance} " +
                    $"which is lower than '{notificationImportance}' required by the notification");
            }

            var smallIconResource = this.GetSmallIconResource(data, context);

            var notificationBuilder = new NotificationCompat.Builder(context, notificationChannel.ChannelId)
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
                var showWhenVisible = GetShowWhenVisible(data);
                notificationBuilder.SetShowWhen(showWhenVisible);
            }

            var largeIconBitmap = this.GetLargeIconBitmap(data, context);
            if (largeIconBitmap != null)
            {
                notificationBuilder.SetLargeIcon(largeIconBitmap);
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

            if (useBigTextStyle && Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
            {
                // Using BigText notification style to support long message
                var style = new NotificationCompat.BigTextStyle();
                style.BigText(messageBody);
                notificationBuilder.SetStyle(style);
            }

            // TODO: Move this logic to Android's FirebasePushNotificationManager

            var category = GetCategoryValue(data);

            if (!string.IsNullOrEmpty(category))
            {
                var allNotificationCategories = CrossFirebasePushNotification.Current.NotificationCategories;
                if (allNotificationCategories is { Length: > 0 })
                {
                    var notificationCategory = allNotificationCategories
                        .SingleOrDefault(c =>
                            string.Equals(c.CategoryId, category, StringComparison.InvariantCultureIgnoreCase));

                    if (notificationCategory != null)
                    {
                        foreach (var notificationAction in notificationCategory.Actions)
                        {
                            var aRequestCode = Guid.NewGuid().GetHashCode();

                            Intent actionIntent;
                            PendingIntent pendingActionIntent;
                            if (notificationAction.Type == NotificationActionType.Foreground)
                            {
                                actionIntent = CreateActivityLaunchIntent(context);

                                if (FirebasePushNotificationManager.NotificationActivityFlags != null)
                                {
                                    actionIntent.SetFlags(FirebasePushNotificationManager.NotificationActivityFlags
                                        .Value);
                                }

                                extras.PutString(Constants.NotificationActionId, notificationAction.Id);
                                actionIntent.PutExtras(extras);
                                pendingActionIntent = PendingIntent.GetActivity(context, aRequestCode, actionIntent,
                                    PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
                            }
                            else
                            {
                                actionIntent = new Intent(context, typeof(PushNotificationActionReceiver));
                                extras.PutString(Constants.NotificationActionId, notificationAction.Id);
                                actionIntent.PutExtras(extras);
                                pendingActionIntent = PendingIntent.GetBroadcast(context, aRequestCode, actionIntent,
                                    PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
                            }

                            var icon = context.Resources.GetIdentifier(notificationAction.Icon ?? "", "drawable", context.PackageName);
                            var action = new NotificationCompat.Action.Builder(icon, notificationAction.Title, pendingActionIntent)
                                .Build();
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
            var largeIconResource = FirebasePushNotificationManager.LargeIconResource;

            try
            {
                if (data.TryGetString(Constants.LargeIconKey, out var largeIcon) && largeIcon != null)
                {
                    largeIconResource = context.Resources.GetIdentifier(largeIcon, "drawable", context.PackageName);
                    if (largeIconResource == 0)
                    {
                        largeIconResource = context.Resources.GetIdentifier(largeIcon, "mipmap", context.PackageName);
                    }
                }

                if (largeIconResource != 0)
                {
                    var name = context.Resources.GetResourceName(largeIconResource);
                    if (name == null)
                    {
                        largeIconResource = 0;
                    }
                }
            }
            catch (Resources.NotFoundException ex)
            {
                largeIconResource = 0;
                this.logger.LogError(ex, "Failed to get large icon");
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

        private int GetSmallIconResource(IDictionary<string, object> data, Context context)
        {
            var smallIconResource = 0;

            try
            {
                if (data.TryGetString(Constants.IconKey, out var icon) && icon != null)
                {
                    try
                    {
                        smallIconResource = context.Resources.GetIdentifier(icon, "drawable", context.PackageName);
                        if (smallIconResource == 0)
                        {
                            smallIconResource = context.Resources.GetIdentifier(icon, "mipmap", context.PackageName);
                        }
                    }
                    catch (Resources.NotFoundException ex)
                    {
                        this.logger.LogError(ex, "Failed to get icon from Resources");
                    }
                }

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
                        this.logger.LogError(ex, "Failed to get default notification icon from meta-data");
                    }
                }

                if (smallIconResource == 0)
                {
                    smallIconResource = context.ApplicationInfo.Icon;
                }
                else
                {
                    var name = context.Resources.GetResourceName(smallIconResource);
                    if (name == null)
                    {
                        smallIconResource = context.ApplicationInfo.Icon;
                    }
                }
            }
            catch (Resources.NotFoundException ex)
            {
                smallIconResource = context.ApplicationInfo.Icon;
                this.logger.LogError(ex, "Failed to get icon from ApplicationInfo");
            }

            return smallIconResource;
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

        private static Intent CreateActivityLaunchIntent(Context context)
        {
            Intent activityIntent;

            if (typeof(Activity).IsAssignableFrom(FirebasePushNotificationManager.NotificationActivityType))
            {
                activityIntent = new Intent(context, FirebasePushNotificationManager.NotificationActivityType);
            }
            else
            {
                activityIntent = FirebasePushNotificationManager.DefaultNotificationActivityType == null
                    ? context.PackageManager.GetLaunchIntentForPackage(context.PackageName)
                    : new Intent(context, FirebasePushNotificationManager.DefaultNotificationActivityType);
            }

            return activityIntent;
        }

        private static bool GetShowWhenVisible(IDictionary<string, object> data)
        {
            bool showWhenVisible;
            if (data.TryGetBool(Constants.ShowWhenKey, out var shouldShowWhen))
            {
                showWhenVisible = shouldShowWhen;
            }
            else
            {
                showWhenVisible = FirebasePushNotificationManager.ShouldShowWhen;
            }

            return showWhenVisible;
        }

        private static NotificationChannelRequest GetChannelOrDefault(IDictionary<string, object> data)
        {
            var notificationChannel = GetChannel(data);
            if (notificationChannel == null)
            {
                var notificationChannels = NotificationChannels.Current.Channels;
                notificationChannel = notificationChannels.Single(c => c.IsDefault);
            }

            return notificationChannel;
        }

        private static NotificationChannelRequest GetChannel(IDictionary<string, object> data)
        {
            NotificationChannelRequest notificationChannel = null;

            if (data.TryGetString(Constants.ChannelIdKey, out var channelId))
            {
                var notificationChannels = NotificationChannels.Current.Channels;
                notificationChannel = notificationChannels.SingleOrDefault(c =>
                    string.Equals(c.ChannelId, channelId, StringComparison.InvariantCultureIgnoreCase));
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

        private NotificationImportance GetNotificationImportance(IDictionary<string, object> data)
        {
            NotificationImportance notificationImportance;
            if (data.TryGetString(Constants.PriorityKey, out var priorityValue))
            {
                notificationImportance = GetNotificationImportance(priorityValue);
            }
            else
            {
                notificationImportance = this.options.Android.DefaultNotificationChannelImportance;
            }

            return notificationImportance;
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
            string category;

            if (data.TryGetString(Constants.ClickActionKey, out var clickActionValue))
            {
                category = clickActionValue;
            }
            else if (data.TryGetString(Constants.CategoryKey, out var categoryValue))
            {
                category = categoryValue;
            }
            else
            {
                category = null;
            }

            return category;
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
        public virtual void OnBuildNotification(NotificationCompat.Builder notificationBuilder,
            IDictionary<string, object> data)
        {
        }

        private static bool IsInForeground()
        {
            var myProcess = new ActivityManager.RunningAppProcessInfo();
            ActivityManager.GetMyMemoryState(myProcess);
            var isInForeground = myProcess.Importance == Android.App.Importance.Foreground;
            return isInForeground;
        }
    }
}