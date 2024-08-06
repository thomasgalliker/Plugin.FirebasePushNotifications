using Android.App;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using Plugin.FirebasePushNotifications.Extensions;

namespace Plugin.FirebasePushNotifications.Platforms.Channels
{
    public class NotificationChannels : INotificationChannels
    {
        private static readonly Lazy<INotificationChannels> Implementation = new Lazy<INotificationChannels>(CreateNotificationChannels, LazyThreadSafetyMode.PublicationOnly);

        public static INotificationChannels Current
        {
            get => Implementation.Value;
        }

        private static INotificationChannels CreateNotificationChannels()
        {
#if ANDROID
            return new NotificationChannels();
#else
            throw Exceptions.NotImplementedInReferenceAssembly();
#endif
        }

        /// <inheritdoc />
        public IEnumerable<NotificationChannelRequest> Channels { get; private set; } = Array.Empty<NotificationChannelRequest>();

        /// <inheritdoc />
        public void CreateChannels(NotificationChannelRequest[] notificationChannelRequests)
        {
            FirebasePushNotificationAndroidOptions.EnsureNotificationChannelRequests(
                    notificationChannelRequests,
                    $"{nameof(this.CreateChannels)}",
                    nameof(notificationChannelRequests));

            this.CreateChannelsInternal(notificationChannelRequests);
        }

        private void CreateChannelsInternal(NotificationChannelRequest[] notificationChannelRequests)
        {
            // TODO: Compare new code with old code

            //#if ANDROID26_0_OR_GREATER
            //            if (Build.VERSION.SdkInt >= BuildVersionCodes.O && createDefaultNotificationChannel)
            //            {
            //                // Create channel to show notifications.
            //                var channelId = DefaultNotificationChannelId;
            //                var channelName = DefaultNotificationChannelName;
            //                var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);

            //                var defaultSoundUri = SoundUri ?? RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            //                var attributes = new AudioAttributes.Builder()
            //                    .SetUsage(AudioUsageKind.Notification)
            //                    .SetContentType(AudioContentType.Sonification)
            //                    .SetLegacyStreamType(Android.Media.Stream.Notification)
            //                    .Build();

            //                var notificationChannel = new NotificationChannel(channelId, channelName, DefaultNotificationChannelImportance);
            //                notificationChannel.EnableLights(true);
            //                notificationChannel.SetSound(defaultSoundUri, attributes);

            //                notificationManager.CreateNotificationChannel(notificationChannel);
            //            }
            //#endif

            if (!OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                return;
            }

            var notificationManager = NotificationManagerCompat.From(Android.App.Application.Context);

            foreach (var notificationChannelRequest in notificationChannelRequests)
            {
                var notificationChannel = new NotificationChannel(notificationChannelRequest.ChannelId, notificationChannelRequest.ChannelName, notificationChannelRequest.Importance);
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

                notificationManager.CreateNotificationChannel(notificationChannel);
            }

            this.Channels = notificationChannelRequests;
        }

        public void UpdateChannels()
        {
            this.UpdateChannels(this.Channels.ToArray());
        }

        public void UpdateChannels(NotificationChannelRequest[] notificationChannelRequests)
        {
            FirebasePushNotificationAndroidOptions.EnsureNotificationChannelRequests(
                 notificationChannelRequests,
                 $"{nameof(UpdateChannels)}",
                 nameof(notificationChannelRequests));

            this.DeleteAllChannels();
            this.CreateChannelsInternal(notificationChannelRequests.Where(c => c.IsActive).ToArray());
        }

        public void DeleteAllChannels()
        {
            this.DeleteChannelsInternals(this.Channels.ToArray(), Array.Empty<NotificationChannelRequest>());
        }

        public void DeleteChannels(string[] channelIds)
        {
            var fork = this.Channels.Fork(c => channelIds.Contains(c.ChannelId)).ToArray();
            var notificationChannelsToDelete = fork.Items1;
            var notificationChannelsToKeep = fork.Items2;

            FirebasePushNotificationAndroidOptions.EnsureNotificationChannelRequests(
                 notificationChannelsToKeep,
                 $"{nameof(this.DeleteChannels)}",
                 nameof(channelIds));

            this.DeleteChannelsInternals(notificationChannelsToDelete, notificationChannelsToKeep);
        }

        private void DeleteChannelsInternals(
            NotificationChannelRequest[] notificationChannelsToDelete,
            NotificationChannelRequest[] notificationChannelsToKeep)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var notificationManager = NotificationManagerCompat.From(Android.App.Application.Context);
            foreach (var notificationChannelRequest in notificationChannelsToDelete)
            {
                notificationManager.DeleteNotificationChannel(notificationChannelRequest.ChannelId);
            }

            this.Channels = notificationChannelsToKeep;
        }
    }
}