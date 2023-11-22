using Android.App;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;

namespace Plugin.FirebasePushNotifications.Platforms.Channels
{
    public static class StaticNotificationChannels
    {
        public static void UpdateChannels(params NotificationChannelRequest[] allChannels)
        {
            DeleteChannels(allChannels);
            CreateChannels(allChannels.Where(c => c.IsActive).ToArray());
        }

        public static void CreateChannels(params NotificationChannelRequest[] notificationChannelRequests)
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
        }

        private static void DeleteChannels(IEnumerable<NotificationChannelRequest> notificationChannelRequests)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var notificationManager = NotificationManagerCompat.From(Android.App.Application.Context);
            foreach (var notificationChannelRequest in notificationChannelRequests)
            {
                notificationManager.DeleteNotificationChannel(notificationChannelRequest.ChannelId);
            }
        }
    }
}