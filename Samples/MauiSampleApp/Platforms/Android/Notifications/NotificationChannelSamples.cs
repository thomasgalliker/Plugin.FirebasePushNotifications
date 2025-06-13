using Android.App;
using Plugin.FirebasePushNotifications.Platforms.Channels;

namespace MauiSampleApp.Platforms.Notifications
{
    public static class NotificationChannelSamples
    {
        public static NotificationChannelRequest Default { get; } = new NotificationChannelRequest
        {
            ChannelId = "default_channel_id",
            ChannelName = "Default Channel",
            Description = "The default notification channel",
            LockscreenVisibility = NotificationVisibility.Public,
            Importance = NotificationImportance.High,
            IsDefault = true,
        };

        public static IEnumerable<NotificationChannelRequest> GetAll()
        {
            yield return Default;

            yield return new NotificationChannelRequest
            {
                ChannelId = "test_channel_1",
                ChannelName = "Test Channel 1",
                Description = "Low priority test channel",
                LockscreenVisibility = NotificationVisibility.Public,
                Importance = NotificationImportance.Low,
            };

            yield return new NotificationChannelRequest
            {
                ChannelId = "test_channel_2",
                ChannelName = "Test Channel 2",
                Description = "Default priority test channel",
                LockscreenVisibility = NotificationVisibility.Public,
                Importance = NotificationImportance.Default,
            };

            yield return new NotificationChannelRequest
            {
                ChannelId = "test_channel_3",
                ChannelName = "Test Channel 3",
                Description = "High priority test channel",
                LockscreenVisibility = NotificationVisibility.Public,
                Importance = NotificationImportance.High,
            };

            yield return new NotificationChannelRequest
            {
                ChannelId = "test_channel_4",
                ChannelName = "Test Channel 4",
                Description = "Test channel assigned to group 1",
                Group = NotificationChannelGroupSamples.TestGroup1.GroupId,
                LockscreenVisibility = NotificationVisibility.Public,
                Importance = NotificationImportance.High,
            };

            yield return new NotificationChannelRequest
            {
                ChannelId = "test_channel_5",
                ChannelName = "Test Channel 5",
                Description = "Test channel assigned to group 1",
                Group = NotificationChannelGroupSamples.TestGroup1.GroupId,
                LockscreenVisibility = NotificationVisibility.Public,
                Importance = NotificationImportance.High,
            };
        }
    }
}