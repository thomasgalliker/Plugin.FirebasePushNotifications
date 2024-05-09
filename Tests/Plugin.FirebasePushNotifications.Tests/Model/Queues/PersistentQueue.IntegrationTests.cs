using FluentAssertions;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Internals;
using Plugin.FirebasePushNotifications.Model.Queues;
using Plugin.FirebasePushNotifications.Tests.Logging;
using Xunit.Abstractions;

namespace Plugin.FirebasePushNotifications.Tests.Model.Queues
{
    [Collection("DisableParallelization")]
    public class PersistentQueueIntegrationTests
    {
        private readonly ILogger<PersistentQueue<TestItem>> logger;
        private readonly IFileInfo fileInfo1;
        private readonly IFileInfo fileInfo2;
        private readonly IFileInfo fileInfo3;

        public PersistentQueueIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            this.logger = new TestOutputHelperLogger<PersistentQueue<TestItem>>(testOutputHelper);

            this.fileInfo1 = new FileInfoWrapper(new FileInfo(Path.Combine(".", "file1.json")));
            this.fileInfo2 = new FileInfoWrapper(new FileInfo(Path.Combine(".", "file2.json")));
            this.fileInfo3 = new FileInfoWrapper(new FileInfo(Path.Combine(".", "file3.json")));

            this.DeleteTestFiles();
        }

        private void DeleteTestFiles()
        {
            this.fileInfo1.Delete();
            this.fileInfo2.Delete();
            this.fileInfo3.Delete();
        }

        [Fact]
        public void ShouldEnqueueItem()
        {
            // Arrange
            var persistentQueue = new PersistentQueue<TestItem>(this.logger, this.fileInfo1);
            persistentQueue.Clear();

            // Act
            persistentQueue.Enqueue(new TestItem { Id = 1 });

            // Assert
            persistentQueue.Count.Should().Be(1);
        }

        [Fact]
        public void ShouldTryDequeueItem()
        {
            // Arrange
            var persistentQueue = new PersistentQueue<TestItem>(this.logger, this.fileInfo1);
            persistentQueue.Clear();

            persistentQueue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = persistentQueue.TryDequeue(out var dequeueItem);

            // Assert
            persistentQueue.Count.Should().Be(0);
            success.Should().BeTrue();
            dequeueItem.Should().NotBeNull();
            dequeueItem.Id.Should().Be(1);
        }

        [Fact]
        public void ShouldEnqueueAndDequeueItems_WithMultipleQueues_SameFile()
        {
            // Arrange
            var persistentQueue1 = new PersistentQueue<TestItem>(this.logger, this.fileInfo1);
            persistentQueue1.Clear();

            persistentQueue1.Enqueue(new TestItem { Id = 1 });
            persistentQueue1.Enqueue(new TestItem { Id = 2 });
            persistentQueue1.Enqueue(new TestItem { Id = 3 });

            // Act
            var persistentQueue2 = new PersistentQueue<TestItem>(this.logger, this.fileInfo1);
            var success1 = persistentQueue2.TryDequeue(out var dequeued1);
            var success2 = persistentQueue2.TryDequeue(out var dequeued2);

            // Assert
            success1.Should().BeTrue();
            dequeued1.Id.Should().Be(1);

            success2.Should().BeTrue();
            dequeued2.Id.Should().Be(2);

            var persistentQueue3 = new PersistentQueue<TestItem>(this.logger, this.fileInfo1);
            persistentQueue3.Count.Should().Be(1);
        }

        [Fact]
        public void ShouldEnqueueAndDequeueItems_WithMultipleQueues_DifferentFiles()
        {
            // Arrange
            var persistentQueue1 = new PersistentQueue<TestItem>(this.logger, this.fileInfo1);
            persistentQueue1.Clear();

            persistentQueue1.Enqueue(new TestItem { Id = 1 });
            persistentQueue1.Enqueue(new TestItem { Id = 2 });
            persistentQueue1.Enqueue(new TestItem { Id = 3 });

            // Act
            var persistentQueue2 = new PersistentQueue<TestItem>(this.logger, this.fileInfo2);
            var success1 = persistentQueue2.TryDequeue(out var dequeued1);
            var success2 = persistentQueue2.TryDequeue(out var dequeued2);

            // Assert
            success1.Should().BeFalse();
            dequeued1.Should().BeNull();

            success2.Should().BeFalse();
            dequeued2.Should().BeNull();

            var persistentQueue3 = new PersistentQueue<TestItem>(this.logger, this.fileInfo3);
            persistentQueue3.Count.Should().Be(0);
        }

        [Fact]
        public void ShouldPeekItem()
        {
            // Arrange
            var persistentQueue = new PersistentQueue<TestItem>(this.logger, this.fileInfo1);
            persistentQueue.Clear();

            persistentQueue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = persistentQueue.TryPeek(out var peekItem);

            // Assert
            persistentQueue.Count.Should().Be(1);
            success.Should().BeTrue();
            peekItem.Id.Should().Be(1);
        }

        [Fact]
        public void ShouldTryPeekItem()
        {
            // Arrange
            var persistentQueue = new PersistentQueue<TestItem>(this.logger, this.fileInfo1);
            persistentQueue.Clear();

            persistentQueue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = persistentQueue.TryPeek(out var peekItem);

            // Assert
            persistentQueue.Count.Should().Be(1);
            success.Should().BeTrue();
            peekItem.Should().NotBeNull();
            peekItem.Id.Should().Be(1);
        }
    }

    [CollectionDefinition("DisableParallelization", DisableParallelization = true)]
    public class DisableParallelization
    {
    }
}