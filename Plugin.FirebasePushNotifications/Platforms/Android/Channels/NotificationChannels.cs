using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Extensions;

namespace Plugin.FirebasePushNotifications.Platforms.Channels
{
    public class NotificationChannels : INotificationChannels
    {
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

        private readonly ILogger<NotificationChannels> logger;
        private readonly NotificationManagerCompat notificationManager;

        private NotificationChannels(
            ILogger<NotificationChannels> logger)
        {
            this.logger = logger;
            this.notificationManager = NotificationManagerCompat.From(Android.App.Application.Context);
        }

        /// <inheritdoc />
        public IEnumerable<NotificationChannel> Channels
        {
            get => this.notificationManager.NotificationChannels;
        }

        /// <inheritdoc />
        public IEnumerable<NotificationChannelGroup> ChannelGroups
        {
            get => this.notificationManager.NotificationChannelGroups;
        }

        /// <inheritdoc />
        public void CreateNotificationChannelGroups([NotNull] NotificationChannelGroupRequest[] notificationChannelGroupRequests)
        {
            if (notificationChannelGroupRequests == null)
            {
                throw new ArgumentNullException(nameof(notificationChannelGroupRequests));
            }

            var groupIds = notificationChannelGroupRequests
                .Select(c => c.GroupId)
                .ToArray();

            this.logger.LogDebug($"CreateNotificationChannelGroups: notificationChannelGroupRequests=[{string.Join(",", groupIds)}]");

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            if (notificationChannelGroupRequests.Length == 0)
            {
                return;
            }

            foreach (var notificationChannelGroupRequest in notificationChannelGroupRequests)
            {
                var notificationChannelGroup = new NotificationChannelGroup(
                    notificationChannelGroupRequest.GroupId,
                    notificationChannelGroupRequest.Name);

                if (notificationChannelGroupRequest.Description is string description)
                {
                    notificationChannelGroup.Description = description;
                }

                this.notificationManager.CreateNotificationChannelGroup(notificationChannelGroup);
            }
        }

        public void SetNotificationChannelGroups([NotNull] NotificationChannelGroupRequest[] notificationChannelGroupRequests)
        {
            if (notificationChannelGroupRequests == null)
            {
                throw new ArgumentNullException(nameof(notificationChannelGroupRequests));
            }

            var groupIds = notificationChannelGroupRequests
                .Select(c => c.GroupId)
                .ToArray();

            this.logger.LogDebug($"SetNotificationChannelGroups: notificationChannelGroupRequests=[{string.Join(",", groupIds)}]");

            if (notificationChannelGroupRequests.Length == 0)
            {
                return;
            }

            var notificationChannelGroupIdsToDelete = this.ChannelGroups
                .Where(g => !groupIds.Contains(g.Id))
                .Select(g => g.Id)
                .ToArray();

            this.DeleteNotificationChannelGroups(notificationChannelGroupIdsToDelete);
            this.CreateNotificationChannelGroups(notificationChannelGroupRequests);
        }

        /// <inheritdoc />
        public void DeleteNotificationChannelGroup(string groupId)
        {
            this.DeleteNotificationChannelGroups(new[] { groupId });
        }

        /// <inheritdoc />
        public void DeleteAllNotificationChannelGroups() // TODO: Fix inconsistent naming
        {
            this.logger.LogDebug("DeleteAllNotificationChannelGroups");

            var groupIds = this.ChannelGroups
                .Select(g => g.Id)
                .ToArray();

            this.DeleteNotificationChannelGroups(groupIds);
        }

        /// <inheritdoc />
        public void DeleteNotificationChannelGroups([NotNull] string[] groupIds)
        {
            if (groupIds == null)
            {
                throw new ArgumentNullException(nameof(groupIds));
            }

            this.logger.LogDebug($"DeleteNotificationChannelGroups: groupIds=[{string.Join(",", groupIds)}]");

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            if (groupIds.Length == 0)
            {
                return;
            }

            foreach (var groupId in groupIds)
            {
                this.notificationManager.DeleteNotificationChannelGroup(groupId);
            }
        }

        /// <inheritdoc />
        public void SetNotificationChannels([NotNull] NotificationChannelRequest[] notificationChannelRequests)
        {
            if (notificationChannelRequests == null)
            {
                throw new ArgumentNullException(nameof(notificationChannelRequests));
            }

            var channelIds = notificationChannelRequests
                .Select(c => c.ChannelId)
                .ToArray();

            this.logger.LogDebug($"SetNotificationChannels: notificationChannelRequests=[{string.Join(",", channelIds)}]");

            if (channelIds.Length == 0)
            {
                return;
            }

            var notificationChannelIdsToDelete = this.Channels
                .Where(c => !channelIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToArray();

            this.DeleteNotificationChannels(notificationChannelIdsToDelete);
            this.CreateNotificationChannelsInternal(notificationChannelRequests);
        }

        /// <inheritdoc />
        public void CreateNotificationChannels([NotNull] NotificationChannelRequest[] notificationChannelRequests)
        {
            if (notificationChannelRequests == null)
            {
                throw new ArgumentNullException(nameof(notificationChannelRequests));
            }

            this.logger.LogDebug(
                $"CreateChannels: " +
                $"notificationChannelRequests=[{string.Join(",", notificationChannelRequests.Select(c => c.ChannelId))}]");

            this.CreateNotificationChannelsInternal(notificationChannelRequests);
        }

        private void CreateNotificationChannelsInternal(NotificationChannelRequest[] notificationChannelRequests)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            foreach (var notificationChannelRequest in notificationChannelRequests)
            {
                var notificationChannel = new NotificationChannel(
                    notificationChannelRequest.ChannelId,
                    notificationChannelRequest.ChannelName,
                    notificationChannelRequest.Importance);

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
                    this.notificationManager.GetNotificationChannelGroup(notificationChannelGroup) == null)
                {
                    this.logger.LogError(
                        $"Attempting to create notification channel {notificationChannelRequest.ChannelId}: " +
                        $"Notification channel group {notificationChannelGroup} not found!");
                }
                else
                {
                    this.notificationManager.CreateNotificationChannel(notificationChannel);
                }
            }
        }

        /// <inheritdoc />
        public void DeleteAllNotificationChannels()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var channelIds = this.Channels.Select(c => c.Id).ToArray();
            this.DeleteNotificationChannels(channelIds);
        }

        /// <inheritdoc />
        public void DeleteNotificationChannels([NotNull] string[] channelIds)
        {
            if (channelIds == null)
            {
                throw new ArgumentNullException(nameof(channelIds));
            }

            this.logger.LogDebug($"DeleteChannels: channelIds=[{string.Join(",", channelIds)}]");

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            if (channelIds.Length == 0)
            {
                return;
            }

            var notificationChannelIdsToDelete = this.Channels
                .Where(c => channelIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToArray();

            foreach (var notificationChannelId in notificationChannelIdsToDelete)
            {
                this.notificationManager.DeleteNotificationChannel(notificationChannelId);
            }
        }

        public void OpenNotificationChannelSettings([NotNull] string channelId)
        {
            if (channelId == null)
            {
                throw new ArgumentNullException(nameof(channelId));
            }

            try
            {
                var context = Android.App.Application.Context;
                var newIntent = new Intent(Settings.ActionChannelNotificationSettings);
                newIntent.PutExtra(Settings.ExtraAppPackage, context.PackageName);
                newIntent.PutExtra(Settings.ExtraChannelId, channelId);
                context.StartActivity(newIntent);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "OpenSettings failed with exception");
            }
        }
    }
}