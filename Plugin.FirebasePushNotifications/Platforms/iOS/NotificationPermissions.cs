using Foundation;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Model;
using UserNotifications;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public class NotificationPermissions : INotificationPermissions
    {
        private const UNAuthorizationOptions AuthorizationOptions =
            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;

        private readonly ILogger logger;

        public NotificationPermissions(ILogger<NotificationPermissions> logger)
        {
            this.logger = logger;
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
        public async Task<bool> RequestPermissionAsync()
        {
            var (granted, error) = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(AuthorizationOptions);
            if (error != null)
            {
                var exception = new Exception("RequestPermissionAsync failed with exception", new NSErrorException(error));
                this.logger.LogError(exception, exception.Message);
                throw exception;
            }

            return granted;
        }
    }
}