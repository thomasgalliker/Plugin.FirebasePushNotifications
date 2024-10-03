namespace Plugin.FirebasePushNotifications.Platforms
{
    public class FirebasePushNotificationiOSOptions
    {
        /// <summary>
        /// Workaround for
        /// https://github.com/thomasgalliker/Plugin.FirebasePushNotifications/issues/70
        /// </summary>
        public iOS18Workaround iOS18Workaround { get; set; } = new iOS18Workaround
        {
            Enabled = true,
            WillPresentNotificationExpirationTime = TimeSpan.FromMilliseconds(1000)
        };
    }

    /// <summary>
    /// Workaround for
    /// https://github.com/thomasgalliker/Plugin.FirebasePushNotifications/issues/70
    /// </summary>
    public class iOS18Workaround
    {
        public bool Enabled { get; set; }

        public TimeSpan WillPresentNotificationExpirationTime { get; set; }
    }
}