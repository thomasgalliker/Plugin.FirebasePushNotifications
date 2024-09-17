using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Plugin.FirebasePushNotifications.Model;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public class NotificationPermissions : INotificationPermissions
    {
        private readonly NotificationManager notificationManager;

        public NotificationPermissions()
        {
            this.notificationManager = (NotificationManager)global::Android.App.Application.Context.GetSystemService(Context.NotificationService);
        }

        /// <inheritdoc/>
        public async Task<bool> RequestPermissionAsync()
        {
            var isPermissionGranted = true;

            // Notification permissions must only be requested
            // on Android API >= 33 (Tiramisu).
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                var permissionStatus = await Permissions.CheckStatusAsync<PostNotificationsPermission>();
                if (permissionStatus != PermissionStatus.Granted)
                {
                    permissionStatus = await Permissions.RequestAsync<PostNotificationsPermission>();
                }

                isPermissionGranted = permissionStatus == PermissionStatus.Granted;
            }

            return isPermissionGranted;
        }

        /// <inheritdoc/>
        public Task<AuthorizationStatus> GetAuthorizationStatusAsync()
        {
            var areNotificationsEnabled = this.AreNotificationsEnabled();
            var authorizationStatus = areNotificationsEnabled
                ? AuthorizationStatus.Granted
                : AuthorizationStatus.Denied;

            return Task.FromResult(authorizationStatus);
        }

        /// <inheritdoc/>
        public async Task<AuthorizationStatus> GetAuthorizationStatusAsync(string[] groupIds, string[] channelIds)
        {
            var authorizationStatus = await this.GetAuthorizationStatusAsync();
            if (authorizationStatus == AuthorizationStatus.Granted)
            {
                var isAnyGroupBlocked = this.IsAnyGroupBlocked(groupIds);
                var isAnyChannelBlock = this.IsAnyChannelBlocked(channelIds);
                authorizationStatus = isAnyGroupBlocked == false && isAnyChannelBlock == false
                    ? AuthorizationStatus.Granted
                    : AuthorizationStatus.Denied;
            }

            return authorizationStatus;
        }

        /// <summary>
        /// Checks if any of the given groupIds is disabled.
        /// </summary>
        private bool IsAnyGroupBlocked(string[] groupIds)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            {
                if (groupIds != null && this.notificationManager.NotificationChannelGroups is IEnumerable<NotificationChannelGroup> groupList)
                {
                    foreach (var group in groupList)
                    {
                        foreach (var groupId in groupIds)
                        {
                            if (group.Id == groupId)
                            {
                                if (group.IsBlocked)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if any of the given channelIds is disabled.
        /// </summary>
        private bool IsAnyChannelBlocked(string[] channelIds)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                if (channelIds != null)
                {
                    foreach (var channelId in channelIds)
                    {
                        var channel = this.notificationManager.GetNotificationChannel(channelId);
                        if (channel != null && channel.Importance == NotificationImportance.None)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool AreNotificationsEnabled()
        {
            bool areNotificationsEnabled;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                areNotificationsEnabled = this.notificationManager.AreNotificationsEnabled();
            }
            else
            {
                var notificationManager = NotificationManagerCompat.From(global::Android.App.Application.Context);
                areNotificationsEnabled = notificationManager.AreNotificationsEnabled();
            }

            return areNotificationsEnabled;
        }
    }
}