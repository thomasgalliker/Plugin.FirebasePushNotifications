using System.Runtime.Versioning;
using Plugin.FirebasePushNotifications.Model;

namespace Plugin.FirebasePushNotifications
{
    public interface INotificationPermissions
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="INotificationPermissions"/>.
        /// </summary>
        public static INotificationPermissions Current => CrossNotificationPermissions.Current;

        /// <summary>
        /// Gets the authorization status for notification permissions.
        /// </summary>
        /// <returns>The authorization status, see <see cref="AuthorizationStatus"/>.</returns>
        Task<AuthorizationStatus> GetAuthorizationStatusAsync();

        /// <inheritdoc cref="GetAuthorizationStatusAsync()"/>
        /// <param name="groupIds">Additionally, checks if the group identifiers are authorized to receive notifications (Android only).</param>
        /// <param name="channelIds">Additionally, checks if the channel identifiers are authorized to receive notifications (Android only).</param>
        [SupportedOSPlatform("android26.0")]
        Task<AuthorizationStatus> GetAuthorizationStatusAsync(string[] groupIds, string[] channelIds);

        /// <summary>
        /// Requests permissions to receive notifications.
        /// </summary>
        /// <returns><c>true</c> if the user has granted the permission, or <c>false</c> if the permission was denied.</returns>
        Task<bool> RequestPermissionAsync();
    }
}