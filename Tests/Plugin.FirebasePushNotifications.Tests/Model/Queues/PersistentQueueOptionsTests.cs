using FluentAssertions;
using Plugin.FirebasePushNotifications.Model.Queues;

namespace Plugin.FirebasePushNotifications.Tests.Model.Queues
{
    public class PersistentQueueOptionsTests
    {
        [Fact]
        public void ShouldCreatePersistentQueueOptions()
        {
            // Act
            var persistentQueueOptions = PersistentQueueOptions.Default;

            // Assert
            persistentQueueOptions.BaseDirectory.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ShouldSetBaseDirectory()
        {
            // Arrange
            var persistentQueueOptions = new PersistentQueueOptions();

            // Act
            persistentQueueOptions.BaseDirectory = ".";

            // Assert
            persistentQueueOptions.BaseDirectory.Should().Be(".");
        }
    }
}