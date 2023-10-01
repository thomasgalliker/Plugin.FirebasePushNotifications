using FluentAssertions;
using Plugin.FirebasePushNotifications.Model;

namespace Plugin.FirebasePushNotifications.Tests.Model
{
    public class PersistentQueueTests
    {
        [Fact]
        public void ShouldCreateQueueWithDefaultOptions()
        {
            // Act
            var queue = new PersistentQueue<TestItem>();

            // Assert
            queue.Count.Should().Be(0);
        }

        [Fact]
        public void ShouldEnqueueAndDequeue()
        {
            // Arrange
            var options = new PersistentQueueOptions();

            var persistentQueue1 = new PersistentQueue<TestItem>(options);
            persistentQueue1.Clear();

            // Act
            persistentQueue1.Enqueue(new TestItem { Id = 1 });
            persistentQueue1.Enqueue(new TestItem { Id = 2 });
            persistentQueue1.Enqueue(new TestItem { Id = 3 });

            var persistentQueue2 = new PersistentQueue<TestItem>(options);
            var dequeued1 = persistentQueue2.Dequeue();
            var dequeued2 = persistentQueue2.Dequeue();

            // Assert
            dequeued1.Id.Should().Be(1);
            dequeued2.Id.Should().Be(2);
            persistentQueue2.Count.Should().Be(1);
        }

        internal class TestItem
        {
            public int Id { get; set; }
        }
    }
}