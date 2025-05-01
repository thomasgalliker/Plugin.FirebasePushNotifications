using System.Collections;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Plugin.FirebasePushNotifications.Platforms.Channels
{
    public class NotificationChannels : INotificationChannels
    {
        private static readonly Lazy<INotificationChannels> Implementation =
            new Lazy<INotificationChannels>(CreateNotificationChannelsInstance, LazyThreadSafetyMode.PublicationOnly);

        public static INotificationChannels Current
        {
            get => Implementation.Value;
        }

        private static INotificationChannels CreateNotificationChannelsInstance()
        {
#if ANDROID
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<NotificationChannels>>();
            return new NotificationChannels(logger);
#else
            throw Exceptions.NotImplementedInReferenceAssembly();
#endif
        }

        private readonly ILogger logger;
        private readonly NotificationManagerCompat notificationManager;

        private NotificationChannels(
            ILogger<NotificationChannels> logger)
        {
            this.logger = logger;
            this.notificationManager = NotificationManagerCompat.From(Android.App.Application.Context);
            this.Channels = new NotificationChannelsDelegate(() => this.notificationManager.NotificationChannels);
        }

        /// <inheritdoc />
        public NotificationChannelsDelegate Channels { get; }

        /// <inheritdoc />
        public IEnumerable<NotificationChannelGroup> ChannelGroups
        {
            get => this.notificationManager.NotificationChannelGroups;
        }

        /// <inheritdoc />
        public void SetNotificationChannelGroups([NotNull] NotificationChannelGroupRequest[] notificationChannelGroupRequests)
        {
            ArgumentNullException.ThrowIfNull(notificationChannelGroupRequests);

            var groupIds = notificationChannelGroupRequests
                .Select(c => c.GroupId)
                .ToArray();

            this.logger.LogDebug($"SetNotificationChannelGroups: notificationChannelGroupRequests=[{string.Join(",", groupIds)}]");

            var notificationChannelGroupsToDelete = this.ChannelGroups;

            if (groupIds.Length > 0)
            {
                notificationChannelGroupsToDelete = notificationChannelGroupsToDelete
                    .Where(c => !groupIds.Contains(c.Id));
            }

            var notificationChannelGroupIdsToDelete = notificationChannelGroupsToDelete
                .Select(c => c.Id)
                .ToArray();

            this.DeleteNotificationChannelGroups(notificationChannelGroupIdsToDelete);
            this.CreateNotificationChannelGroups(notificationChannelGroupRequests);
        }

        /// <inheritdoc />
        public void CreateNotificationChannelGroups([NotNull] NotificationChannelGroupRequest[] notificationChannelGroupRequests)
        {
            ArgumentNullException.ThrowIfNull(notificationChannelGroupRequests);

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

        /// <inheritdoc />
        public void DeleteNotificationChannelGroup(string groupId)
        {
            this.DeleteNotificationChannelGroups(new[] { groupId });
        }

        /// <inheritdoc />
        public void DeleteAllNotificationChannelGroups()
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
            ArgumentNullException.ThrowIfNull(groupIds);

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
            ArgumentNullException.ThrowIfNull(notificationChannelRequests);

            var notificationChannelRequestIds = notificationChannelRequests
                .Select(c => c.ChannelId)
                .ToArray();

            this.logger.LogDebug(
                $"SetNotificationChannels: notificationChannelRequests=[{string.Join(",", notificationChannelRequestIds)}]");

            var notificationChannels = notificationChannelRequests
                .Select(c => (c.ChannelId, c.IsDefault))
                .ToArray();

            EnsureNotificationChannelRequests(
                notificationChannels,
                nameof(this.SetNotificationChannels),
                nameof(notificationChannelRequests));

            var notificationChannelIdsToDelete = this.Channels
                .Select(c => c.Id)
                .Intersect(notificationChannelRequestIds, StringComparer.InvariantCultureIgnoreCase)
                .ToArray();

            this.DeleteNotificationChannelsInternal(notificationChannelIdsToDelete);
            this.CreateNotificationChannelsInternal(notificationChannelRequests);
        }

        /// <inheritdoc />
        public void CreateNotificationChannels([NotNull] NotificationChannelRequest[] notificationChannelRequests)
        {
            ArgumentNullException.ThrowIfNull(notificationChannelRequests);

            var newChannelIds = notificationChannelRequests
                .Select(c => c.ChannelId)
                .ToArray();

            this.logger.LogDebug($"CreateNotificationChannels: notificationChannelRequests=[{string.Join(",", newChannelIds)}]");

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            if (newChannelIds.Length == 0)
            {
                return;
            }

            this.CreateNotificationChannelsInternal(notificationChannelRequests);
        }

        private void CreateNotificationChannelsInternal([NotNull] NotificationChannelRequest[] notificationChannelRequests)
        {
            var existingNotificationChannelIds = this.Channels
                .Select(c => c.Id)
                .ToArray();

            var defaultNotificationChannelId = this.Channels.DefaultNotificationChannelId;

            var newNotificationChannels = existingNotificationChannelIds
                .Select(c => (c, string.Equals(c, defaultNotificationChannelId, StringComparison.InvariantCultureIgnoreCase)))
                .Concat(notificationChannelRequests.Select(c => (c.ChannelId, c.IsDefault)))
                .ToArray();

            EnsureNotificationChannelRequests(
                newNotificationChannels,
                nameof(this.CreateNotificationChannels),
                nameof(notificationChannelRequests));

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

            var defaultNotificationChannelRequest = notificationChannelRequests.SingleOrDefault(c => c.IsDefault);
            if (defaultNotificationChannelRequest != null)
            {
                this.Channels.SetDefaultNotificationChannelIdInternal(defaultNotificationChannelRequest.ChannelId);
            }
        }

        /// <inheritdoc />
        public void DeleteAllNotificationChannels()
        {
            this.logger.LogDebug("DeleteAllNotificationChannels");

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var allNotificationChannelIds = this.Channels
                .Select(c => c.Id)
                .ToArray();

            this.DeleteNotificationChannelsInternal(allNotificationChannelIds);
        }

        /// <inheritdoc />
        public void DeleteNotificationChannel([NotNull] string notificationChannelId)
        {
            this.DeleteNotificationChannels(new[] { notificationChannelId });
        }

        /// <inheritdoc />
        public void DeleteNotificationChannels([NotNull] string[] notificationChannelIds)
        {
            ArgumentNullException.ThrowIfNull(notificationChannelIds);

            this.logger.LogDebug($"DeleteNotificationChannels: notificationChannelIds=[{string.Join(",", notificationChannelIds)}]");

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var channelIds = this.Channels
                .Select(c => c.Id)
                .ToArray();

            var remainingNotificationChannelIds = channelIds
                .Except(notificationChannelIds, StringComparer.InvariantCultureIgnoreCase)
                .ToArray();

            var defaultNotificationChannelId = this.Channels.DefaultNotificationChannelId;

            var remainingNotificationChannels = remainingNotificationChannelIds
                .Select(c => (c, string.Equals(c, defaultNotificationChannelId, StringComparison.InvariantCultureIgnoreCase)))
                .ToArray();

            EnsureNotificationChannelRequests(
                remainingNotificationChannels,
                nameof(this.DeleteNotificationChannels),
                nameof(notificationChannelIds));

            this.DeleteNotificationChannelsInternal(notificationChannelIds);
        }

        private void DeleteNotificationChannelsInternal([NotNull] string[] notificationChannelIdsToDelete)
        {
            if (notificationChannelIdsToDelete.Length == 0)
            {
                return;
            }

            foreach (var notificationChannelId in notificationChannelIdsToDelete)
            {
                try
                {
                    this.notificationManager.DeleteNotificationChannel(notificationChannelId);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"DeleteNotificationChannels failed to delete notificationChannelId={notificationChannelId}");
                }
            }

            var defaultNotificationChannelId = this.Channels.DefaultNotificationChannelId;

            if (notificationChannelIdsToDelete.Contains(defaultNotificationChannelId))
            {
                this.Channels.SetDefaultNotificationChannelIdInternal(null);
            }
        }

        public void OpenNotificationSettings()
        {
            this.logger.LogDebug("OpenNotificationSettings");

            try
            {
                var context = Android.App.Application.Context;
                var newIntent = new Intent(Settings.ActionAppNotificationSettings);
                newIntent.SetFlags(ActivityFlags.NewTask);
                newIntent.PutExtra(Settings.ExtraAppPackage, context.PackageName);
                context.StartActivity(newIntent);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "OpenNotificationSettings failed with exception");
            }
        }

        public void OpenNotificationChannelSettings([NotNull] string notificationChannelId)
        {
            this.logger.LogDebug($"OpenNotificationChannelSettings: notificationChannelId={notificationChannelId}");

            ArgumentNullException.ThrowIfNull(notificationChannelId);

            try
            {
                var context = Android.App.Application.Context;
                var newIntent = new Intent(Settings.ActionChannelNotificationSettings);
                newIntent.SetFlags(ActivityFlags.NewTask);
                newIntent.PutExtra(Settings.ExtraAppPackage, context.PackageName);
                newIntent.PutExtra(Settings.ExtraChannelId, notificationChannelId);
                context.StartActivity(newIntent);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "OpenNotificationChannelSettings failed with exception");
            }
        }

        private static void EnsureNotificationChannelRequests(
            (string ChannelId, bool IsDefault)[] notificationChannels,
            string methodName, string paramName)
        {
            var duplicateChannelIds = notificationChannels
                .Select(c => c.ChannelId)
                .GroupBy(c => c)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();

            if (duplicateChannelIds.Any())
            {
                throw new ArgumentException(
                    $"{methodName} failed: {paramName} contains {nameof(NotificationChannelRequest)} with duplicate {nameof(NotificationChannelRequest.ChannelId)}: " +
                    $"[{string.Join(", ", duplicateChannelIds.Select(id => $"\"{id}\""))}]",
                    paramName);
            }

            var defaultNotificationChannels = notificationChannels.Where(c => c.IsDefault).ToArray();
            if (defaultNotificationChannels.Length > 1)
            {
                throw new ArgumentException(
                    $"{methodName} failed: {paramName} contains more than one active {nameof(NotificationChannelRequest)} with {nameof(NotificationChannelRequest.IsDefault)}=true" +
                    $"[{string.Join(", ", defaultNotificationChannels.Select(c => $"\"{c.ChannelId}\""))}]",
                    paramName);
            }

            if (defaultNotificationChannels.Length < 1)
            {
                throw new ArgumentException(
                    $"{methodName} failed: {paramName} does not contain any active {nameof(NotificationChannelRequest)} with {nameof(NotificationChannelRequest.IsDefault)}=true",
                    paramName);
            }
        }

        public sealed class NotificationChannelsDelegate : IEnumerable<NotificationChannel>
        {
            private readonly Func<IEnumerable<NotificationChannel>> notificationChannels;
            private string defaultNotificationChannelId;

            internal NotificationChannelsDelegate(
                Func<IEnumerable<NotificationChannel>> notificationChannels)
            {
                this.notificationChannels = notificationChannels;
            }

            public NotificationChannel GetDefault()
            {
                if (this.defaultNotificationChannelId is string notificationChannelId)
                {
                    return this.GetById(notificationChannelId);
                }

                return null;
            }

            internal void SetDefaultNotificationChannelIdInternal(string defaultNotificationChannelId)
            {
                this.defaultNotificationChannelId = defaultNotificationChannelId;
            }

            public string DefaultNotificationChannelId
            {
                get => this.defaultNotificationChannelId;
                set
                {
                    if (this.defaultNotificationChannelId != value)
                    {
                        try
                        {
                            var notificationChannel = this.GetById(value);
                            this.defaultNotificationChannelId = notificationChannel.Id;
                        }
                        catch
                        {
                            // Ignore
                        }
                    }
                }
            }

            public IEnumerator<NotificationChannel> GetEnumerator()
            {
                return this.notificationChannels().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public NotificationChannel GetById(string notificationChannelId)
            {
                ArgumentNullException.ThrowIfNull(notificationChannelId);

                var notificationChannel = this.SingleOrDefault(c =>
                    string.Equals(c.Id, notificationChannelId, StringComparison.InvariantCultureIgnoreCase));

                return notificationChannel;
            }
        }
    }
}