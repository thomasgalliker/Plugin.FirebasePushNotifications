#if ANDROID
using Android.App;
using Android.Content;
using Firebase;
using Plugin.FirebasePushNotifications.Platforms.Channels;

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
            set => this.notificationChannels = value ?? Array.Empty<NotificationChannelRequest>();
        }

        public string NotificationTitleKey { get; set; }

        public string NotificationBodyKey { get; set; }

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