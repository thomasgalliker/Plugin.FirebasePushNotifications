namespace Plugin.FirebasePushNotifications
{
    /// <summary>
    /// The notification category type.
    /// </summary>
    /// <remarks>
    /// Only applies to iOS.
    /// </remarks>
    public enum NotificationCategoryType
    {
        /// <summary>
        /// The default notification category type.
        /// </summary>
        Default,

        /// <summary>
        /// The custom notification category type.
        /// </summary>
        /// <remarks>
        /// Only applies to iOS (UNNotificationResponse.IsCustomAction).
        /// </remarks>
        Custom,

        /// <summary>
        /// The dismiss notification category type.
        /// </summary>
        /// <remarks>
        /// Only applies to iOS (UNNotificationResponse.IsDismissAction).
        /// </remarks>
        Dismiss
    }
}
