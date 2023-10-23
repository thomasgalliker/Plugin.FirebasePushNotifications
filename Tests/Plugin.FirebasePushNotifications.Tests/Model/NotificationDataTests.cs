using FluentAssertions;
using Plugin.FirebasePushNotifications.Model;

namespace Plugin.FirebasePushNotifications.Tests.Model
{
    public class NotificationDataTests
    {
        [Fact]
        public void ShouldCreateNotificationData_Empty()
        {
            // Act
            var notificationData = new NotificationData();

            // Assert
            notificationData.Title.Should().BeNull();
            notificationData.Body.Should().BeNull();
        }

        [Fact]
        public void ShouldCreateNotificationData_FromDictionary()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "title", "Title" },
                { "body", "Body" },
            };

            // Act
            var notificationData = new NotificationData(data: data);

            // Assert
            notificationData.Title.Should().Be("Title");
            notificationData.Body.Should().Be("Body");
        }

        [Fact]
        public void ShouldReturnToString()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "title", "Title" },
                { "body", "Body" },
                { "nullProperty", null },
            };
            var notificationData = new NotificationData(data: data);

            // Act
            var toString = notificationData.ToString();

            // Assert
            toString.Should().Be(
                "{title, Title},\r\n" +
                "{body, Body},\r\n" +
                "{nullProperty, null}");
        }
    }
}
