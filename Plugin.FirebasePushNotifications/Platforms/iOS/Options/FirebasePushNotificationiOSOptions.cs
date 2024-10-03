namespace Plugin.FirebasePushNotifications.Platforms
{
    public class FirebasePushNotificationiOSOptions
    {
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
        /// </summary>
        public TimeSpan WillPresentNotificationExpirationTime { get; set; } = TimeSpan.FromSeconds(3);
    }
}