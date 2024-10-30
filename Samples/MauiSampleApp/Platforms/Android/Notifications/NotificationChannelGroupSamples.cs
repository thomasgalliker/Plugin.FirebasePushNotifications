using Plugin.FirebasePushNotifications.Platforms.Channels;

namespace MauiSampleApp.Platforms.Notifications
{
    public static class NotificationChannelGroupSamples
    {
        public static NotificationChannelGroupRequest TestGroup1 { get; } = new NotificationChannelGroupRequest(
            groupId: "test_group_1",
            name: "Test Group 1",
            description: "This is just a test group for demo purposes");

        public static NotificationChannelGroupRequest TestGroup2 { get; } = new NotificationChannelGroupRequest(
            groupId: "test_group_2",
            name: "Test Group 2",
            description: "This is just a test group for demo purposes");

        public static IEnumerable<NotificationChannelGroupRequest> GetAll()
        {
            yield return TestGroup1;
            yield return TestGroup2;
        }
    }
}