#if ANDROID
using Android.App;
using Android.Content;
using Plugin.FirebasePushNotifications.Platforms.Channels;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public class FirebasePushNotificationAndroidOptions
    {
        private NotificationChannelRequest[] notificationChannels = Array.Empty<NotificationChannelRequest>();
        private Type notificationActivityType;

        /// <summary>
        /// The Activity which handles incoming push notifications.
        /// Typically, this is <c>typeof(MainActivity)</c>.
        /// </summary>
        /// <exception cref="ArgumentException">If given type is not an Activity.</exception>
        public virtual Type NotificationActivityType
        {
            get => this.notificationActivityType;
            set
            {
                if (!typeof(Activity).IsAssignableFrom(value))
                {
                    throw new ArgumentException($"{nameof(this.NotificationActivityType)} must be of type {typeof(Activity).FullName}");
                }

                this.notificationActivityType = value;
            }
        }

        public virtual NotificationChannelRequest[] NotificationChannels
        {
            get => this.notificationChannels;
            set
            {
                EnsureNotificationChannelRequests(
                    value,
                    $"{nameof(FirebasePushNotificationOptions)}.{nameof(FirebasePushNotificationOptions.Android)}",
                    nameof(this.NotificationChannels));

                this.notificationChannels = value;
            }
        }

        internal static void EnsureNotificationChannelRequests(NotificationChannelRequest[] notificationChannels, string source, string paramName)
        {
            if (notificationChannels == null)
            {
                throw new ArgumentNullException(paramName, $"{source} must not be null");
            }

            var duplicateChannelIds = notificationChannels
               .Select(c => c.ChannelId)
               .GroupBy(c => c)
               .Where(g => g.Count() > 1)
               .Select(g => g.Key)
               .ToArray();

            if (duplicateChannelIds.Any())
            {
                throw new ArgumentException(
                    $"{source} contains {nameof(NotificationChannelRequest)} with duplicate {nameof(NotificationChannelRequest.ChannelId)}: " +
                    $"[{string.Join(", ", duplicateChannelIds.Select(id => $"\"{id}\""))}]",
                    paramName);
            }

            var defaultNotificationChannels = notificationChannels.Where(c => c.IsDefault && c.IsActive).ToArray();
            if (defaultNotificationChannels.Length > 1)
            {
                throw new ArgumentException(
                    $"{source} contains more than one active {nameof(NotificationChannelRequest)} with {nameof(NotificationChannelRequest.IsDefault)}=true" +
                    $"[{string.Join(", ", defaultNotificationChannels.Select(c => $"\"{c.ChannelId}\""))}]",
                    paramName);
            }
            else if (defaultNotificationChannels.Length < 1)
            {
                throw new ArgumentException(
                    $"{source} does not contain any active {nameof(NotificationChannelRequest)} with {nameof(NotificationChannelRequest.IsDefault)}=true",
                    paramName);
            }
        }

        public string NotificationTitleKey { get; set; }

        public string NotificationBodyKey { get; set; }

        public NotificationImportance DefaultNotificationChannelImportance { get; set; } = NotificationImportance.Default;

        public int? DefaultIconResource { get; set; }

        public int? DefaultLargeIconResource { get; set; }

        public Android.Graphics.Color? DefaultColor { get; set; }

        public ActivityFlags? NotificationActivityFlags { get; set; } = ActivityFlags.ClearTop | ActivityFlags.SingleTop;

        internal Type DefaultNotificationActivityType { get; set; } = null;

        public Android.Net.Uri SoundUri { get; set; }

        public bool ShouldShowWhen { get; set; } = true;

        public bool UseBigTextStyle { get; set; } = true;
    }
}
#endif