using Plugin.FirebasePushNotifications.Model;
using UserNotifications;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public class NotificationPermissions : INotificationPermissions
    {
        public NotificationPermissions()
        {
        }

        /// <inheritdoc/>
        public async Task<AuthorizationStatus> GetAuthorizationStatusAsync()
        {
            return await this.GetAuthorizationStatusAsync(groupIds: null, channelIds: null);
        }

        /// <inheritdoc/>
        public async Task<AuthorizationStatus> GetAuthorizationStatusAsync(string[] groupIds, string[] channelIds)
        {
            var notificationSettings = await UNUserNotificationCenter.Current.GetNotificationSettingsAsync();
            switch (notificationSettings.AuthorizationStatus)
            {
                case UNAuthorizationStatus.Denied:
                    return AuthorizationStatus.Denied;
                case UNAuthorizationStatus.NotDetermined:
                    return AuthorizationStatus.NotDetermined;
                case UNAuthorizationStatus.Authorized:
                case UNAuthorizationStatus.Ephemeral:
                case UNAuthorizationStatus.Provisional:
                    return AuthorizationStatus.Granted;
                default:
                    throw new NotSupportedException($"AuthorizationStatus '{notificationSettings.AuthorizationStatus}' is not supported");
            }
        }

        /// <inheritdoc/>
        public Task<bool> RequestPermissionAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            UNUserNotificationCenter.Current.RequestAuthorization(
                UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                (approved, error) => tcs.TrySetResult(approved)
            );
            return tcs.Task;
        }
    }
}