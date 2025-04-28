using Android.App;
using System.Diagnostics;

namespace Plugin.FirebasePushNotifications.Platforms.Channels
{
    /// <summary>
    /// Notification channel request.
    /// </summary>
    [DebuggerDisplay("{ChannelId}")]
    public class NotificationChannelRequest
    {
        /// <summary>
        /// Sets or gets, the level of interruption of this notification channel.
        /// Default: <c>NotificationImportance.Default</c>
        /// </summary>
        public NotificationImportance Importance { get; set; } = NotificationImportance.Default;

        /// <summary>
        /// The channel identifier.
        /// </summary>
        /// <remarks>
        ///  The channel ID must be unique per package.
        ///  </remarks>
        public string ChannelId { get; init; }

        /// <summary>
        /// The display name of the channel.
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// The description for this channel.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Marks this channel as the default notification channel.
        /// </summary>
        /// <remarks>
        /// Exactly one notification channel has to be marked with IsDefault=true.
        /// </remarks>
        public bool IsDefault { get; set; }

        /// <summary>
        /// The group ID this channel belongs to.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Sets or gets, the notification light color for notifications posted to this channel,
        /// if the device supports that feature
        /// </summary>
        public int LightColor { get; set; }

        /// <summary>
        /// Sound file name for the notification.
        /// </summary>
        public global::Android.Net.Uri SoundUri { get; set; }

        /// <summary>
        /// Only modifiable before the channel is submitted.
        /// </summary>
        public long[] VibrationPattern { get; set; }

        /// <summary>
        /// Sets or gets, whether or not notifications posted to this channel are shown on the lockscreen in full or redacted form.
        /// Default: <c> NotificationVisibility.Public</c>.
        /// </summary>
        public NotificationVisibility LockscreenVisibility { get; set; } = NotificationVisibility.Public;
    }
}