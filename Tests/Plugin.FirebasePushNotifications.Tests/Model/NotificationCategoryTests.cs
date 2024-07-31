using FluentAssertions;
using Newtonsoft.Json;

namespace Plugin.FirebasePushNotifications.Tests.Model
{
    public class NotificationCategoryTests
    {
        [Fact]
        public void ShouldCreateNotificationCategory()
        {
            // Act
            const string categoryId = "category1";
            var notificationActions = new []
            {
                new NotificationAction("action1", "title1", NotificationActionType.Foreground),
                new NotificationAction("action2", "title2", NotificationActionType.Destructive),
            };
            var notificationCategory = new NotificationCategory(categoryId, notificationActions);

            // TODO: Test serialization/deserialization
            var test = JsonConvert.SerializeObject(notificationCategory);

            // Assert
            notificationCategory.CategoryId.Should().Be(categoryId);
            notificationCategory.Actions.Should().HaveCount(notificationActions.Length);
        }

        [Fact]
        public void ShouldCreateNotificationCategory_FromJson()
        {
            // Act
            var notificationCategoryJson = "" +
                "{\"categoryId\":\"category1\"," +
                " \"actions\":[" +
                "   {\"Id\":\"action1\",\"Title\":\"title1\",\"Type\":0,\"Icon\":null}," +
                "   {\"Id\":\"action2\",\"Title\":\"title2\",\"Type\":0,\"Icon\":null}" +
                "  ]," +
                "\"type\":0}";

            var notificationCategory = JsonConvert.DeserializeObject<NotificationCategory>(notificationCategoryJson);

            // Assert
            notificationCategory.CategoryId.Should().Be("category1");
            notificationCategory.Actions.Should().HaveCount(2);

            var action1 = notificationCategory.Actions.ElementAt(0);
            action1.Should().BeEquivalentTo(new NotificationAction("action1", "title1", NotificationActionType.Default));

            var action2 = notificationCategory.Actions.ElementAt(1);
            action2.Should().BeEquivalentTo(new NotificationAction("action2", "title2", NotificationActionType.Default));
        }
    }
}
