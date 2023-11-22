namespace Plugin.FirebasePushNotifications.Model
{
    public enum AuthorizationStatus
    {
        /// <summary>
        /// Did not ask user for this permission (iOS only).
        /// </summary>
        NotDetermined,

        /// <summary>
        /// Notification permission request was denied (or did not ask for permission on Android).
        /// </summary>
        Denied,

        /// <summary>
        /// User has authorized the notification permission request.
        /// </summary>
        Granted
    }
}