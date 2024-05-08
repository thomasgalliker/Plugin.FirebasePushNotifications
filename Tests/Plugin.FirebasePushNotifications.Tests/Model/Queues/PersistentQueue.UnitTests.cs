using System.Text;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Plugin.FirebasePushNotifications.Internals;
using Plugin.FirebasePushNotifications.Model.Queues;

namespace Plugin.FirebasePushNotifications.Tests.Model.Queues
{
    public class PersistentQueueUnitTests
    {
        private readonly AutoMocker autoMocker;

        public PersistentQueueUnitTests()
        {
            this.autoMocker = new AutoMocker();

            var optionsMock = this.autoMocker.GetMock<PersistentQueueOptions>();
            optionsMock.Setup(o => o.BaseDirectory)
                .Returns(".\\BaseDirectory");
            optionsMock.Setup(o => o.FileNameSelector)
                .Returns(t => $"{t.Name}.json");

            var fileInfoMock = this.autoMocker.GetMock<IFileInfo>();
            fileInfoMock.Setup(f => f.Directory)
                .Returns(() =>
                {
                    var currentDictionaryInfoMock = new Mock<IDirectoryInfo>();
                    currentDictionaryInfoMock.Setup(d => d.Exists)
                        .Returns(true);

                    return currentDictionaryInfoMock.Object;
                });

            var fileInfoFactoryMock = this.autoMocker.GetMock<IFileInfoFactory>();
            fileInfoFactoryMock.Setup(f => f.FromPath(It.IsAny<string>()))
                .Returns((string p) =>
                {
                    fileInfoMock.Setup(d => d.FullName)
                        .Returns(p);
                    return fileInfoMock.Object;
                });

            var directoryInfoMock = this.autoMocker.GetMock<IDirectoryInfo>();

            var directoryInfoFactoryMock = this.autoMocker.GetMock<IDirectoryInfoFactory>();
            directoryInfoFactoryMock.Setup(f => f.FromPath(It.IsAny<string>()))
                .Returns((string p) =>
                {
                    directoryInfoMock.Setup(d => d.FullName)
                        .Returns(p);
                    return directoryInfoMock.Object;
                });
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
        public void ShouldCreateQueueWithDefaultOptions()
        {
            // Arrange
            var directoryInfoMock = this.autoMocker.GetMock<IDirectoryInfo>();
            var fileInfoMock = this.autoMocker.GetMock<IFileInfo>();

            // Act
            var queue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);

            // Assert
            queue.Should().NotBeNull();

            directoryInfoMock.Verify(d => d.Exists, Times.Once);
            directoryInfoMock.Verify(d => d.FullName, Times.Once);
            directoryInfoMock.Verify(d => d.Create(), Times.Once);
            directoryInfoMock.VerifyNoOtherCalls();

            fileInfoMock.Verify(d => d.Exists, Times.Once);
            fileInfoMock.VerifyNoOtherCalls();
        }

        [Theory]
        [ClassData(typeof(ValidQueueContentTestData))]
        public void ShouldReadQueueFileContent_ValidContent(string content, int expectedItemsCount)
        {
            // Arrange
            var directoryInfoMock = this.autoMocker.GetMock<IDirectoryInfo>();

            var fileInfoMock = this.autoMocker.GetMock<IFileInfo>();
            fileInfoMock.SetupGet(f => f.Exists)
                .Returns(true);
            fileInfoMock.Setup(f => f.OpenText())
                .Returns(() => GetStreamReaderFromString(content));

            // Act
            var queue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);

            // Assert
            queue.Count.Should().Be(expectedItemsCount);

            fileInfoMock.Verify(f => f.Exists, Times.Once);
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
            var queue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);

            // Assert
            queue.Count.Should().Be(0);

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

            var queue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);

            // Act
            queue.Enqueue(new TestItem { Id = 1 });

            // Assert
            queue.Count.Should().Be(1);

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
            var fileInfoMock = this.autoMocker.GetMock<IFileInfo>();
            var createTextMemoryStreams = SetupCreateText(fileInfoMock);

            var queue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);
            queue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = queue.TryDequeue(out var dequeueItem);

            // Assert
            queue.Count.Should().Be(0);
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

            var queue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);
            queue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = queue.TryDequeue(out var dequeueItem);

            // Assert
            queue.Count.Should().Be(0);
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

            var queue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);
            queue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = queue.TryPeek(out var peekItem);

            // Assert
            queue.Count.Should().Be(1);
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

            var queue = this.autoMocker.CreateInstance<PersistentQueue<TestItem>>(enablePrivate: true);
            queue.Enqueue(new TestItem { Id = 1 });

            // Act
            var success = queue.TryPeek(out var peekItem);

            // Assert
            queue.Count.Should().Be(1);
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