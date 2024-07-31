namespace Plugin.FirebasePushNotifications
{
    public enum NotificationActionType
    {
        /// <summary>
        /// When an action with this notification action type is tapped,
        /// the notification will be handled on background won't bring the application to foreground.
        /// The action will take place without launching the application.
        /// </summary>
        Default = 0,

        /// <summary>
        /// When an action with this notification action type is tapped,
        /// it will bring the application to foreground and process
        /// the notification once application is launched successfully.
        /// </summary>
        Foreground = 1,

        /// <summary>
        /// If set, the user needs to insert the unlock code to launch the action in background.
        /// </summary>
        /// <remarks>
        /// Only applies to iOS (UNNotificationActionOptions.AuthenticationRequired).
        /// </remarks>
        AuthenticationRequired = 2,

        /// <summary>
        /// The notification action type that indicates a destructive action.
        /// </summary>
        /// <remarks>
        /// Only applies to iOS (UNNotificationActionOptions.Destructive).
        /// </remarks>
        Destructive = 3

        //TODO: Android "accept" / "decline" --> decline should be Foreground AND Destructive
    }
}
