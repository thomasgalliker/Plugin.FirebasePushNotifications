using Android.App;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Plugin.FirebasePushNotifications.Extensions;

namespace Plugin.FirebasePushNotifications.Platforms.Channels
{
    public class NotificationChannels : INotificationChannels
    {
        private readonly ILogger<NotificationChannels> logger;

        private static readonly Lazy<INotificationChannels> Implementation =
            new Lazy<INotificationChannels>(CreateNotificationChannels, LazyThreadSafetyMode.PublicationOnly);

        public static INotificationChannels Current
        {
            get => Implementation.Value;
        }

        private static INotificationChannels CreateNotificationChannels()
        {
#if ANDROID
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<NotificationChannels>>();
            return new NotificationChannels(logger);
#else
            throw Exceptions.NotImplementedInReferenceAssembly();
#endif
        }

        private NotificationChannels(ILogger<NotificationChannels> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public IEnumerable<NotificationChannelRequest> Channels { get; private set; } = Array.Empty<NotificationChannelRequest>();

        /// <inheritdoc />
        public void CreateNotificationChannelGroup(NotificationChannelGroupRequest notificationChannelGroupRequest)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            if (notificationChannelGroupRequest == null)
            {
                throw new ArgumentNullException(nameof(notificationChannelGroupRequest));
            }

            this.CreateNotificationChannelGroups(new[] { notificationChannelGroupRequest });
        }

        /// <inheritdoc />
        public void CreateNotificationChannelGroups(NotificationChannelGroupRequest[] notificationChannelGroupRequests)
        {
            this.logger.LogDebug("CreateNotificationChannelGroups");

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            if (notificationChannelGroupRequests.Length == 0)
            {
                return;
            }

            var notificationManager = NotificationManagerCompat.From(Android.App.Application.Context);

            foreach (var notificationChannelGroupRequest in notificationChannelGroupRequests)
            {
                var notificationChannelGroup =
                    new NotificationChannelGroup(notificationChannelGroupRequest.GroupId, notificationChannelGroupRequest.Name);
                if (notificationChannelGroupRequest.Description is string description)
                {
                    notificationChannelGroup.Description = description;
                }

                notificationManager.CreateNotificationChannelGroup(notificationChannelGroup);
            }
        }

        /// <inheritdoc />
        public void DeleteNotificationChannelGroup(string groupId)
        {
            this.DeleteNotificationChannelGroups(new[] { groupId });
        }

        /// <inheritdoc />
        public void DeleteAllNotificationChannelGroups()
        {
            this.logger.LogDebug("DeleteAllNotificationChannelGroups");

            var groupIds = this.Channels
                .Select(c => c.Group)
                .Where(g => g != null)
                .ToArray();
            this.DeleteNotificationChannelGroups(groupIds);
        }

        /// <inheritdoc />
        public void DeleteNotificationChannelGroups(string[] groupIds)
        {
            this.logger.LogDebug("DeleteNotificationChannelGroups");

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var notificationManager = NotificationManagerCompat.From(Android.App.Application.Context);
            foreach (var groupId in groupIds)
            {
                notificationManager.DeleteNotificationChannelGroup(groupId);
            }
        }

        /// <inheritdoc />
        public void CreateChannels(NotificationChannelRequest[] notificationChannelRequests)
        {
            this.logger.LogDebug("CreateChannels");

            FirebasePushNotificationAndroidOptions.EnsureNotificationChannelRequests(
                notificationChannelRequests,
                $"{nameof(this.CreateChannels)}",
                nameof(notificationChannelRequests));

            this.CreateChannelsInternal(notificationChannelRequests);
        }

        private void CreateChannelsInternal(NotificationChannelRequest[] notificationChannelRequests)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var notificationManager = NotificationManagerCompat.From(Android.App.Application.Context);

            foreach (var notificationChannelRequest in notificationChannelRequests)
            {
                var notificationChannel = new NotificationChannel(notificationChannelRequest.ChannelId,
                    notificationChannelRequest.ChannelName, notificationChannelRequest.Importance);
                notificationChannel.Description = notificationChannelRequest.Description;
                notificationChannel.Group = notificationChannelRequest.Group;
                notificationChannel.LightColor = notificationChannelRequest.LightColor;
                notificationChannel.LockscreenVisibility = notificationChannelRequest.LockscreenVisibility;

                var attributes = new AudioAttributes.Builder()
                    .SetUsage(AudioUsageKind.Notification)
                    .SetContentType(AudioContentType.Sonification)
                    .SetLegacyStreamType(global::Android.Media.Stream.Notification)
                    .Build();

                var defaultSoundUri = notificationChannelRequest.SoundUri ?? RingtoneManager.GetDefaultUri(RingtoneType.Notification);
                notificationChannel.SetSound(defaultSoundUri, attributes);

                if (notificationChannelRequest.VibrationPattern != null)
                {
                    notificationChannel.SetVibrationPattern(notificationChannelRequest.VibrationPattern);
                }

                notificationChannel.SetShowBadge(true);
                notificationChannel.EnableLights(true);
                notificationChannel.EnableVibration(true);

                if (notificationChannelRequest.Group is string notificationChannelGroup &&
                    notificationManager.GetNotificationChannelGroup(notificationChannelGroup) == null)
                {
                    this.logger.LogError(
                        $"Attempting to create notification channel {notificationChannelRequest.ChannelId}: " +
                        $"Notification channel group {notificationChannelGroup} not found!");
                }

                notificationManager.CreateNotificationChannel(notificationChannel);
            }

            this.Channels = notificationChannelRequests;
        }

        /// <inheritdoc />
        public void UpdateChannels()
        {
            this.UpdateChannels(this.Channels.ToArray());
        }

        /// <inheritdoc />
        public void UpdateChannels(NotificationChannelRequest[] notificationChannelRequests)
        {
            this.logger.LogDebug("UpdateChannels");

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            FirebasePushNotificationAndroidOptions.EnsureNotificationChannelRequests(
                notificationChannelRequests,
                $"{nameof(UpdateChannels)}",
                nameof(notificationChannelRequests));

            this.DeleteAllChannels();
            this.CreateChannelsInternal(notificationChannelRequests.Where(c => c.IsActive).ToArray());
        }

        /// <inheritdoc />
        public void DeleteAllChannels()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var channelIds = this.Channels.Select(c => c.ChannelId).ToArray();
            this.DeleteChannels(channelIds);
        }

        /// <inheritdoc />
        public void DeleteChannels(string[] channelIds)
        {
            this.logger.LogDebug("DeleteChannels");

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            // TODO: ROLLBACK!! We have to rely on this.Channels!

            var notificationManager = NotificationManagerCompat.From(Android.App.Application.Context);
            var notificationChannels = channelIds.Union(this.Channels.Select(c => c.ChannelId))
                .Select(id => notificationManager.GetNotificationChannel(id))
                .Where(c => c != null)
                .Select(c => new NotificationChannelRequest { ChannelId = c.Id })
                .ToArray();

            var fork = notificationChannels.Fork(c => channelIds.Contains(c.ChannelId)).ToArray();
            var notificationChannelsToDelete = fork.Items1;
            var notificationChannelsToKeep = fork.Items2;

            FirebasePushNotificationAndroidOptions.EnsureNotificationChannelRequests(
                notificationChannelsToKeep,
                $"{nameof(this.DeleteChannels)}",
                nameof(channelIds));


            foreach (var notificationChannelRequest in notificationChannelsToDelete)
            {
                notificationManager.DeleteNotificationChannel(notificationChannelRequest.ChannelId);
            }

            this.Channels = notificationChannelsToKeep;
        }
    }
}