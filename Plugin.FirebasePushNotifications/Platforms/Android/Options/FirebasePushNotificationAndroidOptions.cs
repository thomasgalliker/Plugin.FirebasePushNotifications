#if ANDROID
using Android.App;
using Android.Content;
using Firebase;
using Plugin.FirebasePushNotifications.Platforms.Channels;
using static Plugin.FirebasePushNotifications.Platforms.Channels.NotificationChannelHelper;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public class FirebasePushNotificationAndroidOptions
    {
        private NotificationChannelGroupRequest[] notificationChannelGroups = Array.Empty<NotificationChannelGroupRequest>();
        private NotificationChannelRequest[] notificationChannels = Array.Empty<NotificationChannelRequest>();
        private Type notificationActivityType;

        /// <summary>
        /// This property can be used to configure Firebase programmatically.
        /// By default, this property is <c>null</c> which means,
        /// the google-services.json file with build action GoogleServicesJson
        /// is used to configure Firebase.
        /// </summary>
        public FirebaseOptions FirebaseOptions { get; set; }

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

        /// <summary>
        /// The initial list of notification channel groups configured at app startup time.
        /// </summary>
        public virtual NotificationChannelGroupRequest[] NotificationChannelGroups
        {
            get => this.notificationChannelGroups;
            set => this.notificationChannelGroups = value ?? Array.Empty<NotificationChannelGroupRequest>();
        }

        /// <summary>
        /// The initial list of notification channels configured at app startup time.
        /// </summary>
        public virtual NotificationChannelRequest[] NotificationChannels
        {
            get => this.notificationChannels;
            set
            {
                value ??= Array.Empty<NotificationChannelRequest>();

                var notificationChannels = value
                    .Select(c => (c.ChannelId, c.IsDefault))
                    .ToArray();

                var checkResult = CheckNotificationChannelRequests(notificationChannels, nameof(this.NotificationChannels), nameof(value));
                if (checkResult.Result != NotificationChannelCheckResult.Success &&
                    checkResult.Result != NotificationChannelCheckResult.NoDefaultChannel)
                {
                    throw checkResult.Exception;
                }

                this.notificationChannels = value;
            }
        }

        public string NotificationTitleKey { get; set; }

        public string NotificationBodyKey { get; set; }

        /// <summary>
        /// The notification importance used by default
        /// - if the notification data does not contain any "priority" flags.
        /// - as notification importance for the default notification channel (if not specified).
        /// Default value: <c>NotificationImportance.Default</c>
        /// </summary>
        public NotificationImportance DefaultNotificationImportance { get; set; } = NotificationImportance.Default;

        public int? DefaultIconResource { get; set; }

        public int? DefaultLargeIconResource { get; set; }

        public Android.Graphics.Color? DefaultColor { get; set; }

        public ActivityFlags? NotificationActivityFlags { get; set; } = ActivityFlags.ClearTop | ActivityFlags.SingleTop;

        public Android.Net.Uri SoundUri { get; set; }

        public bool ShouldShowWhen { get; set; } = true;

        public bool UseBigTextStyle { get; set; } = true;
    }
}
#endif