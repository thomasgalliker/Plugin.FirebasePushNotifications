using FluentAssertions;
using Plugin.FirebasePushNotifications.Model.Queues;

namespace Plugin.FirebasePushNotifications.Tests.Model.Queues
{
    public class PersistentQueueIntegrationTests
    {
        [Fact]
        public void ShouldEnqueueItem()
        {
            // Arrange
            var queue = new PersistentQueue<TestItem>("key");
            queue.Clear();

            // Act
            queue.Enqueue(new TestItem { Id = 1 });

            // Assert
            queue.Count.Should().Be(1);
        }

        [Fact]
        public void ShouldTryDequeueItem()
        {
            // Arrange
            var queue = new PersistentQueue<TestItem>("key");
            queue.Clear();

            queue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = queue.TryDequeue(out var dequeueItem);

            // Assert
            queue.Count.Should().Be(0);
            success.Should().BeTrue();
            dequeueItem.Should().NotBeNull();
            dequeueItem.Id.Should().Be(1);
        }

        [Fact]
        public void ShouldEnqueueAndDequeueItems_WithMultipleQueues()
        {
            // Arrange
            var options = new PersistentQueueOptions();

            var persistentQueue1 = new PersistentQueue<TestItem>("key", options);
            persistentQueue1.Clear();

            persistentQueue1.Enqueue(new TestItem { Id = 1 });
            persistentQueue1.Enqueue(new TestItem { Id = 2 });
            persistentQueue1.Enqueue(new TestItem { Id = 3 });

            // Act
            var persistentQueue2 = new PersistentQueue<TestItem>("key", options);
            var success1 = persistentQueue2.TryDequeue(out var dequeued1);
            var success2 = persistentQueue2.TryDequeue(out var dequeued2);

            // Assert
            success1.Should().BeTrue();
            dequeued1.Id.Should().Be(1);

            success2.Should().BeTrue();
            dequeued2.Id.Should().Be(2);

            var persistentQueue3 = new PersistentQueue<TestItem>("key", options);
            persistentQueue3.Count.Should().Be(1);
        }

        [Fact]
        public void ShouldEnqueueAndTryDequeueItems_WithMultipleQueues()
        {
            // Arrange
            var options = new PersistentQueueOptions();

            var persistentQueue1 = new PersistentQueue<TestItem>("key", options);
            persistentQueue1.Clear();

            persistentQueue1.Enqueue(new TestItem { Id = 1 });
            persistentQueue1.Enqueue(new TestItem { Id = 2 });
            persistentQueue1.Enqueue(new TestItem { Id = 3 });

            // Act
            var persistentQueue2 = new PersistentQueue<TestItem>("key", options);
            var dequeuedSuccess1 = persistentQueue2.TryDequeue(out var dequeuedItem1);
            var dequeuedSuccess2 = persistentQueue2.TryDequeue(out var dequeuedItem2);

            // Assert
            dequeuedSuccess1.Should().BeTrue();
            dequeuedItem1.Should().NotBeNull();
            dequeuedItem1.Id.Should().Be(1);

            dequeuedSuccess2.Should().BeTrue();
            dequeuedItem2.Should().NotBeNull();
            dequeuedItem2.Id.Should().Be(2);

            var persistentQueue3 = new PersistentQueue<TestItem>("key", options);
            persistentQueue3.Count.Should().Be(1);
        }

        [Fact]
        public void ShouldPeekItem()
        {
            // Arrange
            var queue = new PersistentQueue<TestItem>("key");
            queue.Clear();

            queue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = queue.TryPeek(out var peekItem);

            // Assert
            queue.Count.Should().Be(1);
            success.Should().BeTrue();
            peekItem.Id.Should().Be(1);
        }

        [Fact]
        public void ShouldTryPeekItem()
        {
            // Arrange
            var queue = new PersistentQueue<TestItem>("key");
            queue.Clear();

            queue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = queue.TryPeek(out var peekItem);

            // Assert
            queue.Count.Should().Be(1);
            success.Should().BeTrue();
            peekItem.Should().NotBeNull();
            peekItem.Id.Should().Be(1);
        }
    }
}