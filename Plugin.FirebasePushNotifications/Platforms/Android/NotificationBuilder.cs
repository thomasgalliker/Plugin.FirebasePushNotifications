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
using static Android.App.ActivityManager;
using Application = Android.App.Application;
using Debug = System.Diagnostics.Debug;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public class NotificationBuilder : INotificationBuilder
    {
        private readonly ILogger<NotificationBuilder> logger;
        private readonly FirebasePushNotificationOptions options;

        public NotificationBuilder(
            ILogger<NotificationBuilder> logger,
            FirebasePushNotificationOptions options)
        {
            this.logger = logger;
            this.options = options;
        }

        public virtual void OnNotificationReceived(IDictionary<string, object> data)
        {
            this.logger.LogDebug("OnNotificationReceived");

            // TODO / WARNING:
            // This piece of code is full of errors and contradictions.
            // We need to find out which pieces are still needed and which are obsolete.
            // Long term goal: A developer can use IPushNotificationHandler to intercept all notifications and do some operations on them.
            // All the logic in here should move to the Android-specific implementation of FirebasePushNotificationManager.

            if (data.TryGetBool(Constants.SilentKey, out var silentValue) && silentValue)
            {
                return;
            }

            var isForeground = IsInForeground();
            var hasChannelId = data.TryGetString(Constants.ChannelIdKey, out var channelId);

            data.TryGetString(Constants.PriorityKey, out var priorityValue);
            var priority = GetNotificationImportance(priorityValue);

            var isNotHighOrMax = FirebasePushNotificationManager.DefaultNotificationChannelImportance != NotificationImportance.High && FirebasePushNotificationManager.DefaultNotificationChannelImportance != NotificationImportance.Max;

            if (isForeground && (hasChannelId || priority != NotificationImportance.High && priority != NotificationImportance.Max && !isNotHighOrMax))
            {
                return;
            }

            var context = Application.Context;

            // TODO: Cleanup these variables. There is a lot of legacy code from the Xamarin plugin here.

            var notificationId = 0;
            var showWhenVisible = FirebasePushNotificationManager.ShouldShowWhen;
            var useBigTextStyle = FirebasePushNotificationManager.UseBigTextStyle;
            var soundUri = FirebasePushNotificationManager.SoundUri;
            var largeIconResource = FirebasePushNotificationManager.LargeIconResource;
            var smallIconResource = FirebasePushNotificationManager.IconResource;
            var notificationColor = FirebasePushNotificationManager.Color;

            if (data.TryGetString(Constants.IdKey, out var id))
            {
                try
                {
                    notificationId = Convert.ToInt32(id);
                }
                catch (Exception ex)
                {
                    // Keep the default value of zero for the notify_id, but log the conversion problem.
                    Debug.WriteLine($"Failed to convert {id} to an integer {ex}");
                }
            }

            if (data.TryGetBool(Constants.ShowWhenKey, out var shouldShowWhen))
            {
                showWhenVisible = shouldShowWhen;
            }

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
                Debug.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            soundUri ??= RingtoneManager.GetDefaultUri(RingtoneType.Notification);

            try
            {
                if (data.TryGetString(Constants.IconKey, out var icon) && icon != null)
                {
                    try
                    {
                        smallIconResource = context.Resources.GetIdentifier(icon, "drawable", Application.Context.PackageName);
                        if (smallIconResource == 0)
                        {
                            smallIconResource = context.Resources.GetIdentifier(icon, "mipmap", Application.Context.PackageName);
                        }
                    }
                    catch (Resources.NotFoundException ex)
                    {
                        Debug.WriteLine(ex.ToString());
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
                Debug.WriteLine(ex.ToString());
            }

            try
            {
                if (data.TryGetString(Constants.LargeIconKey, out var largeIcon) && largeIcon != null)
                {
                    largeIconResource = context.Resources.GetIdentifier(largeIcon, "drawable", Application.Context.PackageName);
                    if (largeIconResource == 0)
                    {
                        largeIconResource = context.Resources.GetIdentifier(largeIcon, "mipmap", Application.Context.PackageName);
                    }
                }

                if (largeIconResource > 0)
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
                Debug.WriteLine(ex.ToString());
            }

            if (data.TryGetString(Constants.ColorKey, out var colorValue) && colorValue != null)
            {
                try
                {
                    notificationColor = Android.Graphics.Color.ParseColor(colorValue);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to parse color");
                }
            }

            var resultIntent = typeof(Activity).IsAssignableFrom(FirebasePushNotificationManager.NotificationActivityType)
                ? new Intent(Application.Context, FirebasePushNotificationManager.NotificationActivityType)
                : (FirebasePushNotificationManager.DefaultNotificationActivityType == null
                    ? context.PackageManager.GetLaunchIntentForPackage(context.PackageName)
                    : new Intent(Application.Context, FirebasePushNotificationManager.DefaultNotificationActivityType));

            var extras = new Bundle();
            foreach (var p in data)
            {
                extras.PutString(p.Key, p.Value.ToString());
            }

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
            var requestCode = new Java.Util.Random().NextInt();

            var pendingIntent = PendingIntent.GetActivity(context, requestCode, resultIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            if (string.IsNullOrEmpty(channelId))
            {
                var notificationChannels = NotificationChannels.Current.Channels;
                var defaultNotificationChannelId = notificationChannels.Single(c => c.IsDefault);
                channelId = defaultNotificationChannelId.ChannelId;
            }

            var notificationBuilder = new NotificationCompat.Builder(context, channelId)
                 .SetSmallIcon(smallIconResource)
                 .SetAutoCancel(true)
                 .SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis())
                 .SetContentIntent(pendingIntent);

            var messageTitle = this.GetMessageTitle(data, context);
            if (!string.IsNullOrEmpty(messageTitle))
            {
                notificationBuilder.SetContentTitle(messageTitle);
            }

            var messageBody = this.GetMessageBody(data);
            if (!string.IsNullOrEmpty(messageBody))
            {
                notificationBuilder.SetContentText(messageBody);
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
            {
                notificationBuilder.SetShowWhen(showWhenVisible);
            }

            if (largeIconResource > 0)
            {
                var largeIconBitmap = BitmapFactory.DecodeResource(context.Resources, largeIconResource);
                notificationBuilder.SetLargeIcon(largeIconBitmap);
            }

            var deleteIntent = new Intent(context, typeof(PushNotificationDeletedReceiver));
            deleteIntent.PutExtras(extras);
            var pendingDeleteIntent = PendingIntent.GetBroadcast(context, requestCode, deleteIntent, PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable);
            notificationBuilder.SetDeleteIntent(pendingDeleteIntent);

            if (Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.O)
            {
                if (priority != null)
                {
                    switch (priority.Value)
                    {
                        case NotificationImportance.Max:
                            notificationBuilder.SetPriority(NotificationCompat.PriorityMax);
                            notificationBuilder.SetVibrate(new long[] { 1000, 1000, 1000, 1000, 1000 });
                            break;
                        case NotificationImportance.High:
                            notificationBuilder.SetPriority(NotificationCompat.PriorityHigh);
                            notificationBuilder.SetVibrate(new long[] { 1000, 1000, 1000, 1000, 1000 });
                            break;
                        case NotificationImportance.Default:
                            notificationBuilder.SetPriority(NotificationCompat.PriorityDefault);
                            notificationBuilder.SetVibrate(new long[] { 1000, 1000, 1000, 1000, 1000 });
                            break;
                        case NotificationImportance.Low:
                            notificationBuilder.SetPriority(NotificationCompat.PriorityLow);
                            break;
                        case NotificationImportance.Min:
                            notificationBuilder.SetPriority(NotificationCompat.PriorityMin);
                            break;
                        default:
                            notificationBuilder.SetPriority(NotificationCompat.PriorityDefault);
                            notificationBuilder.SetVibrate(new long[] { 1000, 1000, 1000, 1000, 1000 });
                            break;
                    }
                }
                else
                {
                    notificationBuilder.SetVibrate(new long[] { 1000, 1000, 1000, 1000, 1000 });
                }

                try
                {
                    notificationBuilder.SetSound(soundUri);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to set sound");
                }
            }

            // Try to resolve (and apply) localized parameters
            this.ResolveLocalizedParameters(notificationBuilder, data);

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
                        .SingleOrDefault(c => string.Equals(c.CategoryId, category, StringComparison.InvariantCultureIgnoreCase));

                    if (notificationCategory != null)
                    {
                        foreach (var notificationAction in notificationCategory.Actions)
                        {
                            var aRequestCode = Guid.NewGuid().GetHashCode();

                            Intent actionIntent;
                            PendingIntent pendingActionIntent;
                            if (notificationAction.Type == NotificationActionType.Foreground)
                            {
                                actionIntent = typeof(Activity).IsAssignableFrom(FirebasePushNotificationManager.NotificationActivityType)
                                    ? new Intent(Application.Context, FirebasePushNotificationManager.NotificationActivityType)
                                    : (FirebasePushNotificationManager.DefaultNotificationActivityType == null
                                        ? context.PackageManager.GetLaunchIntentForPackage(context.PackageName)
                                        : new Intent(Application.Context, FirebasePushNotificationManager.DefaultNotificationActivityType));

                                if (FirebasePushNotificationManager.NotificationActivityFlags != null)
                                {
                                    actionIntent.SetFlags(FirebasePushNotificationManager.NotificationActivityFlags.Value);
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

                            // TODO: Replace all calls to Application.Context with context variable!

                            var icon = context.Resources.GetIdentifier(notificationAction.Icon ?? "", "drawable",
                                Application.Context.PackageName);
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

        private string GetMessageBody(IDictionary<string, object> data)
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

        private string GetMessageTitle(IDictionary<string, object> data, Context context)
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
            else
            {
                messageTitle = context.ApplicationInfo.LoadLabel(context.PackageManager);
            }

            return messageTitle;
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

        private static NotificationImportance? GetNotificationImportance(string priorityValue)
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
                    return null;
            }
        }

        /// <summary>
        /// Resolves the localized parameters using the string resources, combining the key and the passed arguments of the notification.
        /// </summary>
        /// <param name="notificationBuilder">Notification builder.</param>
        /// <param name="parameters">Parameters.</param>
        private void ResolveLocalizedParameters(NotificationCompat.Builder notificationBuilder, IDictionary<string, object> parameters)
        {
            // Resolve title localization
            if (parameters.TryGetString("title_loc_key", out var titleKey))
            {
                parameters.TryGetValue("title_loc_args", out var titleArgs);

                var localizedTitle = this.GetLocalizedString(titleKey, titleArgs as string[], notificationBuilder);
                if (localizedTitle != null)
                {
                    notificationBuilder.SetContentTitle(localizedTitle);
                }
            }

            // Resolve body localization
            if (parameters.TryGetString("body_loc_key", out var bodyKey))
            {
                parameters.TryGetValue("body_loc_args", out var bodyArgs);

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

        private static bool IsInForeground()
        {
            bool isInForeground;

            var myProcess = new RunningAppProcessInfo();
            ActivityManager.GetMyMemoryState(myProcess);
            isInForeground = myProcess.Importance == Android.App.Importance.Foreground;

            return isInForeground;
        }
    }
}
