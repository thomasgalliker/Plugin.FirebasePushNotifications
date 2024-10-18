using System.Diagnostics;
using Firebase.Core;
using UserNotifications;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public class FirebasePushNotificationiOSOptions
    {
        public Firebase.Core.Options FirebaseOptions { get; set; }

        /// <summary>
        /// The default presentation options used if app runs in foreground mode
        /// and the notification message does not contain the priority flag.
        /// </summary>
        public UNNotificationPresentationOptions PresentationOptions { get; set; } = UNNotificationPresentationOptions.None;

        /// <summary>
        /// Workaround for
        /// https://github.com/thomasgalliker/Plugin.FirebasePushNotifications/issues/70
        /// </summary>
        public iOS18Workaround iOS18Workaround { get; private set; } = new iOS18Workaround();
    }

    /// <summary>
    /// Workaround for
    /// https://github.com/thomasgalliker/Plugin.FirebasePushNotifications/issues/70
    /// </summary>
    public class iOS18Workaround
    {
        /// <summary>
        /// Enables/disables the workaround for duplicate notifications on iOS 18.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Defines the interval between identical UNNotification.Request.Identifiers,
        /// treating them as duplicates if they occur within this time frame.
        /// Default: 3 seconds
        /// </summary>
        public TimeSpan WillPresentNotificationExpirationTime { get; set; }
            = Debugger.IsAttached ? TimeSpan.FromMinutes(1) : TimeSpan.FromSeconds(3);
    }
}