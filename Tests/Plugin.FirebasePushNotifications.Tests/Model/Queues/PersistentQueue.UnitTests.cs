using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Plugin.FirebasePushNotifications.Internals;
using Plugin.FirebasePushNotifications.Model.Queues;
using Plugin.FirebasePushNotifications.Tests.Logging;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Plugin.FirebasePushNotifications.Tests.Model.Queues
{
    public class PersistentQueueUnitTests
    {
        private readonly AutoMocker autoMocker;
        private readonly ILogger<PersistentQueue<TestItem>> logger;

        public PersistentQueueUnitTests(ITestOutputHelper testOutputHelper)
        {
            this.autoMocker = new AutoMocker();

            this.logger = new TestOutputHelperLogger<PersistentQueue<TestItem>>(testOutputHelper);

            var fileInfoMock = this.autoMocker.GetMock<IFileInfo>();
            fileInfoMock.Setup(f => f.Directory)
                .Returns(() =>
                {
                    var currentDictionaryInfoMock = new Mock<IDirectoryInfo>();
                    currentDictionaryInfoMock.Setup(d => d.Exists)
                        .Returns(true);

                    return currentDictionaryInfoMock.Object;
                });
        }

        [Fact]
        public void ShouldArgumentNullException_IfFileInfoIsNull()
        {
            // Act
            var action = () => new PersistentQueue<TestItem>(null, null);

            // Assert
            var exception = action.Should().Throw<ArgumentNullException>().Which;
            exception.ParamName.Should().Be("fileInfo");
        }

        [Fact]
        public void ShouldCreateQueueWithDefaultOptions()
        {
            // Arrange
            var fileInfoMock = this.autoMocker.GetMock<IFileInfo>();

            // Act
            var persistentQueue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);

            // Assert
            persistentQueue.Should().NotBeNull();

            fileInfoMock.Verify(d => d.Exists, Times.Once);
            fileInfoMock.VerifyNoOtherCalls();
        }

        [Theory]
        [ClassData(typeof(ValidQueueContentTestData))]
        public void ShouldReadQueueFileContent_ValidContent(string content, int expectedItemsCount)
        {
            // Arrange
            var fileInfoMock = this.autoMocker.GetMock<IFileInfo>();
            fileInfoMock.SetupGet(f => f.Exists)
                .Returns(true);
            fileInfoMock.Setup(f => f.OpenText())
                .Returns(() => GetStreamReaderFromString(content));

            // Act
            var persistentQueue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);

            // Assert
            persistentQueue.Count.Should().Be(expectedItemsCount);

            fileInfoMock.Verify(f => f.Exists, Times.Once);
            fileInfoMock.Verify(f => f.FullName, Times.Once);
            fileInfoMock.Verify(f => f.OpenText(), Times.Once);
            fileInfoMock.VerifyNoOtherCalls();
        }

        public class ValidQueueContentTestData : TheoryData<string, int>
        {
            public ValidQueueContentTestData()
            {
                this.Add("[]", 0);
                this.Add("[{Id: 1},{Id: 2}]", 2);
            }
        }

        [Theory]
        [ClassData(typeof(InvalidQueueContentTestData))]
        public void ShouldReadQueueFileContent_InvalidContent(string content)
        {
            // Arrange
            var fileInfoMock = this.autoMocker.GetMock<IFileInfo>();
            fileInfoMock.SetupGet(f => f.Exists)
                .Returns(true);
            fileInfoMock.Setup(f => f.OpenText())
                .Returns(() => GetStreamReaderFromString(content));

            // Act
            var persistentQueue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);

            // Assert
            persistentQueue.Count.Should().Be(0);

            fileInfoMock.Verify(f => f.FullName, Times.Once);
            fileInfoMock.Verify(f => f.Exists, Times.Once);
            fileInfoMock.Verify(f => f.OpenText(), Times.Once);
            fileInfoMock.VerifyNoOtherCalls();
        }

        public class InvalidQueueContentTestData : TheoryData<string>
        {
            public InvalidQueueContentTestData()
            {
                this.Add(null);
                this.Add("");
                this.Add(" ");
                this.Add("invalid content");
            }
        }

        [Fact]
        public void ShouldEnqueueItem()
        {
            // Arrange
            var fileInfoMock = this.autoMocker.GetMock<IFileInfo>();
            var createTextMemoryStreams = SetupCreateText(fileInfoMock);

            var persistentQueue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);

            // Act
            persistentQueue.Enqueue(new TestItem { Id = 1 });

            // Assert
            persistentQueue.Count.Should().Be(1);

            var texts = GetTextFromMemoryStreams(createTextMemoryStreams);
            texts.Should().ContainInOrder(new[]
            {
                "[{\"Id\":1}]",
            });
        }

        [Fact]
        public void ShouldDequeueItem()
        {
            // Arrange
            var directoryInfoFactoryMock = this.autoMocker.GetMock<IDirectoryInfoFactory>();
            var fileInfoFactoryMock = this.autoMocker.GetMock<IFileInfoFactory>();
            var fileInfoMock = this.autoMocker.GetMock<IFileInfo>();
            var createTextMemoryStreams = SetupCreateText(fileInfoMock);

            var persistentQueue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);
            persistentQueue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = persistentQueue.TryDequeue(out var dequeueItem);

            // Assert
            persistentQueue.Count.Should().Be(0);
            success.Should().BeTrue();
            dequeueItem.Id.Should().Be(1);

            var texts = GetTextFromMemoryStreams(createTextMemoryStreams);
            texts.Should().ContainInOrder(new[]
            {
                "[{\"Id\":1}]",
                "[]",
            });
        }

        [Fact]
        public void ShouldTryDequeueItem()
        {
            // Arrange
            var fileInfoMock = this.autoMocker.GetMock<IFileInfo>();
            var createTextMemoryStreams = SetupCreateText(fileInfoMock);

            var persistentQueue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);
            persistentQueue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = persistentQueue.TryDequeue(out var dequeueItem);

            // Assert
            persistentQueue.Count.Should().Be(0);
            success.Should().BeTrue();
            dequeueItem.Should().NotBeNull();
            dequeueItem.Id.Should().Be(1);

            var texts = GetTextFromMemoryStreams(createTextMemoryStreams);
            texts.Should().ContainInOrder(new[]
              {
                "[{\"Id\":1}]",
                "[]",
            });
        }

        [Fact]
        public void ShouldPeekItem()
        {
            // Arrange
            var fileInfoMock = this.autoMocker.GetMock<IFileInfo>();
            var createTextMemoryStreams = SetupCreateText(fileInfoMock);

            var persistentQueue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);
            persistentQueue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = persistentQueue.TryPeek(out var peekItem);

            // Assert
            persistentQueue.Count.Should().Be(1);
            success.Should().BeTrue();
            peekItem.Id.Should().Be(1);

            var texts = GetTextFromMemoryStreams(createTextMemoryStreams);
            texts.Should().ContainInOrder(new[]
              {
                "[{\"Id\":1}]",
            });
        }

        [Fact]
        public void ShouldTryPeekItem()
        {
            // Arrange
            var fileInfoMock = this.autoMocker.GetMock<IFileInfo>();
            var createTextMemoryStreams = SetupCreateText(fileInfoMock);

            var persistentQueue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);
            persistentQueue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = persistentQueue.TryPeek(out var peekItem);

            // Assert
            persistentQueue.Count.Should().Be(1);
            success.Should().BeTrue();
            peekItem.Should().NotBeNull();
            peekItem.Id.Should().Be(1);

            var texts = GetTextFromMemoryStreams(createTextMemoryStreams);
            texts.Should().ContainInOrder(new[]
              {
                "[{\"Id\":1}]",
            });
        }

        private static IEnumerable<MemoryStream> SetupCreateText(Mock<IFileInfo> fileInfoMock)
        {
            var createTextMemoryStreams = new List<MemoryStream>();

            fileInfoMock.Setup(f => f.CreateText())
                .Returns(() =>
                {
                    var memoryStream = new MemoryStream();
                    createTextMemoryStreams.Add(memoryStream);
                    return new StreamWriter(memoryStream);
                });

            return createTextMemoryStreams;
        }

        private static StreamReader GetStreamReaderFromString(string text)
        {
            var bytes = text != null ? Encoding.UTF8.GetBytes(text) : Array.Empty<byte>();
            return new StreamReader(new MemoryStream(bytes));
        }

        private static string[] GetTextFromMemoryStreams(IEnumerable<MemoryStream> createTextMemoryStreams)
        {
            return createTextMemoryStreams
                .Select(m => Encoding.UTF8.GetString(m.ToArray()))
                .ToArray();
        }
    }
}