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
            var notificationActions = new []
            {
                new NotificationAction("action1", "title1", NotificationActionType.Foreground),
                new NotificationAction("action2", "title2", NotificationActionType.Destructive),
            };
            var notificationCategory = new NotificationCategory("category1", notificationActions);

            // Assert
            notificationCategory.CategoryId.Should().Be("category1");
            notificationCategory.Actions.Should().HaveCount(2);
        }

        [Fact]
        public void ShouldSerializeNotificationCategory()
        {
            // Arrange
            var notificationActions = new []
            {
                new NotificationAction("action1", "title1", NotificationActionType.Foreground),
                new NotificationAction("action2", "title2", NotificationActionType.Destructive),
            };
            var notificationCategory = new NotificationCategory("category1", notificationActions);

            // Act
            var notificationCategoryJson = JsonConvert.SerializeObject(notificationCategory);

            // Assert
            notificationCategoryJson.Should().Be(
                "{\"categoryId\":\"category1\"," +
                "\"actions\":[{\"id\":\"action1\"," +
                "\"title\":\"title1\",\"type\":1," +
                "\"icon\":null}," +
                "{\"id\":\"action2\"," +
                "\"title\":\"title2\"," +
                "\"type\":3," +
                "\"icon\":null}]," +
                "\"type\":0}");
        }

        [Fact]
        public void ShouldDeserializeNotificationCategory()
        {
            // Arrange
            const string notificationCategoryJson =
                "{\"categoryId\":\"category1\"," +
                " \"actions\":[" +
                "   {\"Id\":\"action1\",\"Title\":\"title1\",\"Type\":1,\"Icon\":null}," +
                "   {\"Id\":\"action2\",\"Title\":\"title2\",\"Type\":3,\"Icon\":null}" +
                "  ]," +
                "\"type\":2}";

            // Act
            var notificationCategory = JsonConvert.DeserializeObject<NotificationCategory>(notificationCategoryJson);

            // Assert
            notificationCategory.CategoryId.Should().Be("category1");
            notificationCategory.Actions.Should().HaveCount(2);
            notificationCategory.Type.Should().Be(NotificationCategoryType.Dismiss);

            var action1 = notificationCategory.Actions.ElementAt(0);
            action1.Should().BeEquivalentTo(new NotificationAction("action1", "title1", NotificationActionType.Foreground));

            var action2 = notificationCategory.Actions.ElementAt(1);
            action2.Should().BeEquivalentTo(new NotificationAction("action2", "title2", NotificationActionType.Destructive));
        }
    }
}
