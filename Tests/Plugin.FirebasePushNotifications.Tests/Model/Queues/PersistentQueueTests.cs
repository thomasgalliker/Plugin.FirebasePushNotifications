using FluentAssertions;
using Plugin.FirebasePushNotifications.Model.Queues;

namespace Plugin.FirebasePushNotifications.Tests.Model.Queues
{
    public class PersistentQueueTests
    {
        [Fact]
        public void ShouldCreateQueueWithDefaultOptions()
        {
            // Act
            var action = () => new PersistentQueue<TestItem>();

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void ShouldArgumentNullException_IfOptionsIsNull()
        {
            // Act
            var action = () => new PersistentQueue<TestItem>(null);

            // Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ShouldEnqueueItem()
        {
            // Arrange
            var queue = new PersistentQueue<TestItem>();
            queue.Clear();

            // Act
            queue.Enqueue(new TestItem { Id = 1 });

            // Assert
            queue.Count.Should().Be(1);
        }

        [Fact]
        public void ShouldDequeueItem()
        {
            // Arrange
            var queue = new PersistentQueue<TestItem>();
            queue.Clear();

            queue.Enqueue(new TestItem { Id = 1 });

            // Act
            var dequeueItem = queue.Dequeue();

            // Assert
            queue.Count.Should().Be(0);
            dequeueItem.Id.Should().Be(1);
        }


        [Fact]
        public void ShouldTryDequeueItem()
        {
            // Arrange
            var queue = new PersistentQueue<TestItem>();
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

            var persistentQueue1 = new PersistentQueue<TestItem>(options);
            persistentQueue1.Clear();

            persistentQueue1.Enqueue(new TestItem { Id = 1 });
            persistentQueue1.Enqueue(new TestItem { Id = 2 });
            persistentQueue1.Enqueue(new TestItem { Id = 3 });

            // Act
            var persistentQueue2 = new PersistentQueue<TestItem>(options);
            var dequeued1 = persistentQueue2.Dequeue();
            var dequeued2 = persistentQueue2.Dequeue();

            // Assert
            dequeued1.Id.Should().Be(1);
            dequeued2.Id.Should().Be(2);

            var persistentQueue3 = new PersistentQueue<TestItem>(options);
            persistentQueue3.Count.Should().Be(1);
        }

        [Fact]
        public void ShouldEnqueueAndTryDequeueItems_WithMultipleQueues()
        {
            // Arrange
            var options = new PersistentQueueOptions();

            var persistentQueue1 = new PersistentQueue<TestItem>(options);
            persistentQueue1.Clear();

            persistentQueue1.Enqueue(new TestItem { Id = 1 });
            persistentQueue1.Enqueue(new TestItem { Id = 2 });
            persistentQueue1.Enqueue(new TestItem { Id = 3 });

            // Act
            var persistentQueue2 = new PersistentQueue<TestItem>(options);
            var dequeuedSuccess1 = persistentQueue2.TryDequeue(out var dequeuedItem1);
            var dequeuedSuccess2 = persistentQueue2.TryDequeue(out var dequeuedItem2);

            // Assert
            dequeuedSuccess1.Should().BeTrue();
            dequeuedItem1.Should().NotBeNull();
            dequeuedItem1.Id.Should().Be(1);

            dequeuedSuccess2.Should().BeTrue();
            dequeuedItem2.Should().NotBeNull();
            dequeuedItem2.Id.Should().Be(2);

            var persistentQueue3 = new PersistentQueue<TestItem>(options);
            persistentQueue3.Count.Should().Be(1);
        }


        [Fact]
        public void ShouldPeekItem()
        {
            // Arrange
            var queue = new PersistentQueue<TestItem>();
            queue.Clear();

            queue.Enqueue(new TestItem { Id = 1 });

            // Act
            var peekItem = queue.Peek();

            // Assert
            queue.Count.Should().Be(1);
            peekItem.Id.Should().Be(1);
        }

        [Fact]
        public void ShouldTryPeekItem()
        {
            // Arrange
            var queue = new PersistentQueue<TestItem>();
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

        internal class TestItem
        {
            public int Id { get; set; }
        }
    }
}