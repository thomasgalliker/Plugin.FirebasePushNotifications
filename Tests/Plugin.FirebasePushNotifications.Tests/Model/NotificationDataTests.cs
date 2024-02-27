using FluentAssertions;
using Plugin.FirebasePushNotifications.Model;

namespace Plugin.FirebasePushNotifications.Tests.Model
{
    public class NotificationDataTests
    {
        [Fact]
        public void ShouldCreateNotificationData_WithTitleAndBody()
        {
            // Act
            const string title = "Title";
            const string body = "Body";

            var notificationData = new NotificationMessage(title, body);

            // Assert
            notificationData.Title.Should().Be(title);
            notificationData.Body.Should().Be(body);
            notificationData.Data.Should().BeEquivalentTo(new Dictionary<string, string>
            {
                { "title", "Title" },
                { "body", "Body" },
            });
        }

        [Fact]
        public void ShouldCreateNotificationData_WithTitleBodyAndData()
        {
            // Act
            const string title = "Title";
            const string body = "Body";
            var data = new Dictionary<string, string>
            {
                { "key1", "Value1" }
            };

            var notificationData = new NotificationMessage(title, body, data);

            // Assert
            notificationData.Title.Should().Be(title);
            notificationData.Body.Should().Be(body);
            notificationData.Data.Should().BeEquivalentTo(new Dictionary<string, string>
            {
                { "title", "Title" },
                { "body", "Body" },
                { "key1", "Value1" }
            });
        }

        [Fact]
        public void ShouldCreateNotificationData_Empty()
        {
            // Act
            var notificationData = new NotificationMessage(data: null);

            // Assert
            notificationData.Title.Should().BeNull();
            notificationData.Body.Should().BeNull();
            notificationData.Tag.Should().BeNull();
            notificationData.Data.Should().BeEmpty();
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
            var notificationData = new NotificationMessage(data);

            // Assert
            notificationData.Title.Should().Be("Title");
            notificationData.Body.Should().Be("Body");
            notificationData.Tag.Should().BeNull();
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
            var notificationData = new NotificationMessage(data: data);

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
