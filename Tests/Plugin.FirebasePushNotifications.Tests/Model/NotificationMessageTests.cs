using FluentAssertions;
using Plugin.FirebasePushNotifications.Model;

namespace Plugin.FirebasePushNotifications.Tests.Model
{
    public class NotificationMessageTests
    {
        [Fact]
        public void ShouldCreatenotificationMessage_WithTitleAndBody()
        {
            // Act
            const string title = "Title";
            const string body = "Body";

            var notificationMessage = new NotificationMessage(title, body);

            // Assert
            notificationMessage.Title.Should().Be(title);
            notificationMessage.Body.Should().Be(body);
            notificationMessage.Data.Should().BeEquivalentTo(new Dictionary<string, string>
            {
                { "title", "Title" },
                { "body", "Body" },
            });
        }

        [Fact]
        public void ShouldCreatenotificationMessage_WithTitleBodyAndData()
        {
            // Act
            const string title = "Title";
            const string body = "Body";
            var data = new Dictionary<string, string>
            {
                { "key1", "Value1" }
            };

            var notificationMessage = new NotificationMessage(title, body, data);

            // Assert
            notificationMessage.Title.Should().Be(title);
            notificationMessage.Body.Should().Be(body);
            notificationMessage.Data.Should().BeEquivalentTo(new Dictionary<string, string>
            {
                { "title", "Title" },
                { "body", "Body" },
                { "key1", "Value1" }
            });
        }

        [Fact]
        public void ShouldCreatenotificationMessage_Empty()
        {
            // Act
            var notificationMessage = new NotificationMessage(data: null);

            // Assert
            notificationMessage.Title.Should().BeNull();
            notificationMessage.Body.Should().BeNull();
            notificationMessage.Tag.Should().BeNull();
            notificationMessage.Data.Should().BeEmpty();
        }

        [Fact]
        public void ShouldCreatenotificationMessage_FromDictionary()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                { "title", "Title" },
                { "body", "Body" },
            };

            // Act
            var notificationMessage = new NotificationMessage(data);

            // Assert
            notificationMessage.Title.Should().Be("Title");
            notificationMessage.Body.Should().Be("Body");
            notificationMessage.Tag.Should().BeNull();
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
            var notificationMessage = new NotificationMessage(data: data);

            // Act
            var toString = notificationMessage.ToString();

            // Assert
            toString.Should().Be(
                "{title, Title},\r\n" +
                "{body, Body},\r\n" +
                "{nullProperty, null}");
        }
    }
}
