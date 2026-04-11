using System.Text.Json;
using FluentAssertions;

namespace Plugin.FirebasePushNotifications.Tests.Model
{
    public class NotificationActionTests
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        [Fact]
        public void ShouldCreateNotificationAction()
        {
            // Act
            var notificationAction = new NotificationAction("action1", "title1", NotificationActionType.Foreground);

            // Assert
            notificationAction.Id.Should().Be("action1");
            notificationAction.Title.Should().Be("title1");
            notificationAction.Type.Should().Be(NotificationActionType.Foreground);
            notificationAction.Icon.Should().BeNull();
        }

        [Fact]
        public void ShouldSerializeNotificationAction()
        {
            // Arrange
            var notificationAction = new NotificationAction("action1", "title1", NotificationActionType.Foreground);

            // Act
            var notificationActionJson = JsonSerializer.Serialize(notificationAction, JsonSerializerOptions);

            // Assert
            notificationActionJson.Should().Be("{\"id\":\"action1\",\"title\":\"title1\",\"type\":1,\"icon\":null}");
        }

        [Fact]
        public void ShouldDeserializeNotificationAction()
        {
            // Arrange
            const string notificationActionJson = "{\"Id\":\"action1\",\"Title\":\"title1\",\"Type\":1,\"Icon\":null}";

            // Act
            var notificationAction = JsonSerializer.Deserialize<NotificationAction>(notificationActionJson, JsonSerializerOptions);

            // Assert
            notificationAction.Should().NotBeNull();
            notificationAction.Should().BeEquivalentTo(new NotificationAction("action1", "title1", NotificationActionType.Foreground));
        }
    }
}
