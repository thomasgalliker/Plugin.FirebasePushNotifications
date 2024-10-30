using Android.App;
using Plugin.FirebasePushNotifications.Platforms.Channels;

namespace MauiSampleApp.Platforms.Notifications
{
    public static class NotificationChannelSamples
    {
        public static IEnumerable<NotificationChannelRequest> GetAll()
        {
            yield return new NotificationChannelRequest
            {
                ChannelId = "default_channel_id",
                ChannelName = "Default Channel",
                Description = "The default notification channel",
                LockscreenVisibility = NotificationVisibility.Public,
                Importance = NotificationImportance.High,
                IsDefault = true,
            };

            yield return new NotificationChannelRequest
            {
                ChannelId = "test_channel_1",
                ChannelName = "Test Channel 1",
                Description = "Description for test channel 1",
                LockscreenVisibility = NotificationVisibility.Public,
                Importance = NotificationImportance.High,
            };

            yield return new NotificationChannelRequest
            {
                ChannelId = "test_channel_2",
                ChannelName = "Test Channel 2",
                Description = "Description for test channel 2",
                Group = NotificationChannelGroupSamples.TestGroup1.GroupId,
                LockscreenVisibility = NotificationVisibility.Public,
                Importance = NotificationImportance.High,
                IsActive = false,
            };

            yield return new NotificationChannelRequest
            {
                ChannelId = "test_channel_3",
                ChannelName = "Test Channel 3",
                Description = "Description for test channel 3",
                Group = NotificationChannelGroupSamples.TestGroup1.GroupId,
                LockscreenVisibility = NotificationVisibility.Public,
                Importance = NotificationImportance.High,
                IsActive = false,
            };
        }
    }
}