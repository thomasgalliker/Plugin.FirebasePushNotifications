using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Provider;
using AndroidX.Core.App;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Utils;
using static Plugin.FirebasePushNotifications.Platforms.Channels.NotificationChannelHelper;
using Environment = System.Environment;

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
            var firebasePushNotificationOptions = IPlatformApplication.Current.Services.GetRequiredService<FirebasePushNotificationOptions>();
            return new NotificationChannels(logger, firebasePushNotificationOptions);
#else
            throw Exceptions.NotImplementedInReferenceAssembly();
#endif
        }

        private readonly ILogger logger;
        private readonly FirebasePushNotificationOptions options;
        private readonly NotificationManagerCompat notificationManager;

        private NotificationChannels(
            ILogger<NotificationChannels> logger,
            FirebasePushNotificationOptions options)
        {
            this.logger = logger;
            this.options = options;
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
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            ArgumentNullException.ThrowIfNull(notificationChannelGroupRequests);

            var groupIds = notificationChannelGroupRequests
                .Select(c => c.GroupId)
                .ToArray();

            this.logger.LogDebug($"SetNotificationChannelGroups: notificationChannelGroupRequests=[{string.Join(",", groupIds)}]");

            var notificationChannelGroupIdsToDelete = this.ChannelGroups.Select(c => c.Id);

            if (groupIds.Any())
            {
                notificationChannelGroupIdsToDelete = notificationChannelGroupIdsToDelete
                    .Where(g => !groupIds.Contains(g, StringComparer.InvariantCultureIgnoreCase));
            }

            this.DeleteNotificationChannelGroupsInternal(notificationChannelGroupIdsToDelete.ToArray());
            this.CreateNotificationChannelGroupsInternal(notificationChannelGroupRequests);
        }

        /// <inheritdoc />
        public void CreateNotificationChannelGroups([NotNull] NotificationChannelGroupRequest[] notificationChannelGroupRequests)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            ArgumentNullException.ThrowIfNull(notificationChannelGroupRequests);

            var groupIds = notificationChannelGroupRequests
                .Select(c => c.GroupId)
                .ToArray();

            this.logger.LogDebug($"CreateNotificationChannelGroups: notificationChannelGroupRequests=[{string.Join(",", groupIds)}]");

            this.CreateNotificationChannelGroupsInternal(notificationChannelGroupRequests);
        }

        private void CreateNotificationChannelGroupsInternal(NotificationChannelGroupRequest[] notificationChannelGroupRequests)
        {
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
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            this.logger.LogDebug("DeleteAllNotificationChannelGroups");

            var groupIds = this.ChannelGroups
                .Select(g => g.Id)
                .ToArray();

            this.DeleteNotificationChannelGroupsInternal(groupIds);
        }

        /// <inheritdoc />
        public void DeleteNotificationChannelGroups([NotNull] string[] groupIds)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            ArgumentNullException.ThrowIfNull(groupIds);

            this.logger.LogDebug($"DeleteNotificationChannelGroups: groupIds=[{string.Join(",", groupIds)}]");

            this.DeleteNotificationChannelGroupsInternal(groupIds);
        }

        private void DeleteNotificationChannelGroupsInternal(string[] groupIds)
        {
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
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            ArgumentNullException.ThrowIfNull(notificationChannelRequests);

            var notificationChannelRequestIds = notificationChannelRequests
                .Select(c => c.ChannelId)
                .ToArray();

            this.logger.LogDebug(
                $"SetNotificationChannels: notificationChannelRequests=[{string.Join(",", notificationChannelRequestIds)}]");

            // If no default notification channel is requested, we create it with predefined properties.
            if (!notificationChannelRequests.Any(c => c.IsDefault))
            {
                var defaultNotificationChannelId = GetDefaultNotificationChannelIds().First();
                var defaultNotificationChannelRequest = this.CreateDefaultNotificationChannelRequest(defaultNotificationChannelId);

                notificationChannelRequests = notificationChannelRequests
                    .Prepend(defaultNotificationChannelRequest)
                    .ToArray();
            }

            var notificationChannels = notificationChannelRequests
                .Select(c => (c.ChannelId, c.IsDefault))
                .ToArray();

            EnsureNotificationChannelRequests(
                notificationChannels,
                nameof(this.SetNotificationChannels),
                nameof(notificationChannelRequests));

            var notificationChannelIdsToDelete = this.Channels
                .Select(c => c.Id)
                .ToArray();

            this.DeleteNotificationChannelsInternal(notificationChannelIdsToDelete);
            this.CreateNotificationChannelsInternal(notificationChannelRequests);
        }

        public void EnsureDefaultNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var existingNotificationChannelIds = this.Channels
                .Select(c => c.Id)
                .ToArray();

            // There is no concept of default notification channels in Android.
            // Therefore, we need to find or create the default notification channel.
            // 1. If there is no existing notification channel, we create a new one with default properties.
            // 2. If we have exactly one notification channel, we treat it as the default notification channel.
            // 3. If we have multiple notification channels,
            //    3a. try to get the default notification channel from the list of existing notification channels, or
            //    3b. we treat the first in the list as default notification channel.
            var defaultNotificationChannelIds = GetDefaultNotificationChannelIds().ToArray();
            var defaultNotificationChannelId = existingNotificationChannelIds.Length switch
            {
                0 => defaultNotificationChannelIds.First(),
                1 => existingNotificationChannelIds[0],
                _ => existingNotificationChannelIds.FirstOrDefault(c => defaultNotificationChannelIds.Contains(c, StringComparer.InvariantCultureIgnoreCase)) ?? existingNotificationChannelIds[0],
            };

            var defaultNotificationChannelExists = existingNotificationChannelIds
                .Any(c => string.Equals(c, defaultNotificationChannelId, StringComparison.InvariantCultureIgnoreCase));

            this.logger.LogDebug(
                $"EnsureDefaultNotificationChannel: existingNotificationChannelIds=[{string.Join(",", existingNotificationChannelIds)}] " +
                $"--> defaultNotificationChannelId={defaultNotificationChannelId} ({(defaultNotificationChannelExists ? "existing" : "new")})");

            if (!defaultNotificationChannelExists)
            {
                // If no default notification channel exists, we create one with predefined properties.
                var defaultNotificationChannelRequest = this.CreateDefaultNotificationChannelRequest(defaultNotificationChannelId);
                this.CreateNotificationChannelsInternal(new[] { defaultNotificationChannelRequest });
            }
            else
            {
                this.Channels.SetDefaultNotificationChannelIdInternal(defaultNotificationChannelId);
            }
        }

        private NotificationChannelRequest CreateDefaultNotificationChannelRequest(string defaultNotificationChannelId)
        {
            var defaultNotificationChannelRequest = new NotificationChannelRequest
            {
                ChannelId = defaultNotificationChannelId,
                ChannelName = Constants.DefaultNotificationChannelName,
                IsDefault = true,
                LockscreenVisibility = NotificationVisibility.Public,
                Importance = this.options.Android.DefaultNotificationImportance,
            };

            const string optionsPath = $"options.{nameof(FirebasePushNotificationOptions.Android)}." +
                                       $"{nameof(FirebasePushNotificationAndroidOptions.NotificationChannels)}";

            this.logger.LogWarning(
                $"Missing default notification channel (IsDefault=true) in {optionsPath}.{Environment.NewLine}" +
                $"A default notification channel with the following properties will be created: {Environment.NewLine}" +
                $"> ChannelId={defaultNotificationChannelRequest.ChannelId}, {Environment.NewLine}" +
                $"> ChannelName={defaultNotificationChannelRequest.ChannelName}, {Environment.NewLine}" +
                $"> IsDefault=true, {Environment.NewLine}" +
                $"> LockscreenVisibility=NotificationVisibility.Public, {Environment.NewLine}" +
                $"> Importance=NotificationImportance.{defaultNotificationChannelRequest.Importance}");

            return defaultNotificationChannelRequest;
        }

        private static IEnumerable<string> GetDefaultNotificationChannelIds()
        {
            // Try to get the default notification channel ID from AndroidManifest.xml
            {
                var metadata = MetadataHelper.GetMetadata();
                var defaultNotificationChannelId = metadata.GetString(
                    key: Constants.MetadataDefaultNotificationChannelId,
                    defaultValue: null);

                if (!string.IsNullOrEmpty(defaultNotificationChannelId))
                {
                    yield return defaultNotificationChannelId;
                }
            }

            // Try to get the default notification channel ID from static string defined in this library
            {
                var defaultNotificationChannelId = Constants.DefaultNotificationChannelId;
                if (!string.IsNullOrEmpty(defaultNotificationChannelId))
                {
                    yield return defaultNotificationChannelId;
                }
                else
                {
                    yield return "default_channel_id";
                }
            }
        }

        /// <inheritdoc />
        public void CreateNotificationChannels([NotNull] NotificationChannelRequest[] notificationChannelRequests)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            ArgumentNullException.ThrowIfNull(notificationChannelRequests);

            var newChannelIds = notificationChannelRequests
                .Select(c => c.ChannelId)
                .ToArray();

            this.logger.LogDebug($"CreateNotificationChannels: notificationChannelRequests=[{string.Join(",", newChannelIds)}]");

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
                    this.logger.LogDebug($"Creating notification channel '{notificationChannelRequest.ChannelId}'");
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
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            this.logger.LogDebug("DeleteAllNotificationChannels");

            var defaultNotificationChannelId = this.Channels.DefaultNotificationChannelId;

            var allNotificationChannelIds = this.Channels
                .Select(c => c.Id)
                .Where(c => !string.Equals(c, defaultNotificationChannelId, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            this.DeleteNotificationChannelsInternal(allNotificationChannelIds);
        }

        /// <inheritdoc />
        public void DeleteNotificationChannel([NotNull] string notificationChannelId)
        {
            ArgumentNullException.ThrowIfNull(notificationChannelId);

            this.DeleteNotificationChannels(new[] { notificationChannelId });
        }

        /// <inheritdoc />
        public void DeleteNotificationChannels([NotNull] string[] notificationChannelIds)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            ArgumentNullException.ThrowIfNull(notificationChannelIds);

            this.logger.LogDebug($"DeleteNotificationChannels: notificationChannelIds=[{string.Join(",", notificationChannelIds)}]");

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

        private void DeleteNotificationChannelsInternal([NotNull] string[] notificationChannelIds)
        {
            if (notificationChannelIds.Length == 0)
            {
                return;
            }

            foreach (var notificationChannelId in notificationChannelIds)
            {
                try
                {
                    this.notificationManager.DeleteNotificationChannel(notificationChannelId);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"DeleteNotificationChannel failed to delete notificationChannelId={notificationChannelId}");
                }
            }
        }

        private void CreateDefaultNotificationChannelInternal()
        {
            // TODO
        }

        /// <inheritdoc />
        public void OpenNotificationSettings()
        {
            this.logger.LogDebug("OpenNotificationSettings");

            try
            {
                var context = Android.App.Application.Context;

                var intent = new Intent();
                var sdkInt = (int)Build.VERSION.SdkInt;
                if (sdkInt >= (int)BuildVersionCodes.O) // >= Android 8.0
                {
                    intent.SetAction(Settings.ActionAppNotificationSettings);
                    intent.PutExtra(Settings.ExtraAppPackage, context.PackageName);
                    intent.PutExtra(Settings.ExtraChannelId, context.ApplicationInfo!.Uid);
                }
                else if (sdkInt is >= (int)BuildVersionCodes.Lollipop and < (int)BuildVersionCodes.O) // >= Android 5.0 && < Android 8.0
                {
                    intent.SetAction(Settings.ActionAppNotificationSettings);
                    intent.PutExtra("app_package", context.PackageName);
                    intent.PutExtra("app_uid", context.ApplicationInfo!.Uid);
                }
                else
                {
                    return;
                }

                context.StartActivity(intent);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "OpenNotificationSettings failed with exception");
            }
        }

        /// <inheritdoc />
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

        public sealed class NotificationChannelsDelegate : IEnumerable<NotificationChannel>
        {
            private readonly Func<IList<NotificationChannel>> notificationChannels;
            private string defaultNotificationChannelId;

            internal NotificationChannelsDelegate(
                Func<IList<NotificationChannel>> notificationChannels)
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