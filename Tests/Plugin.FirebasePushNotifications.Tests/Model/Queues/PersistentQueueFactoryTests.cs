using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq.AutoMock;
using Plugin.FirebasePushNotifications.Internals;
using Plugin.FirebasePushNotifications.Model.Queues;
using Plugin.FirebasePushNotifications.Tests.Logging;
using Xunit.Abstractions;
using Moq;

namespace Plugin.FirebasePushNotifications.Tests.Model.Queues
{
    public class PersistentQueueFactoryTests
    {
        private readonly AutoMocker autoMocker;

        public PersistentQueueFactoryTests(ITestOutputHelper testOutputHelper)
        {
            this.autoMocker = new AutoMocker();

            this.autoMocker.Use<ILoggerFactory>(new TestOutputHelperLoggerFactory(testOutputHelper));

            var optionsMock = this.autoMocker.GetMock<PersistentQueueOptions>();
            optionsMock.Setup(o => o.BaseDirectory)
                .Returns(".\\BaseDirectory");
            optionsMock.Setup(o => o.FileNameSelector)
                .Returns(t => $"{t.Key}.json");

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
        public void ShouldCreatePersistentQueue()
        {
            // Arrange
            var persistentQueueFactory = this.autoMocker.CreateInstance<PersistentQueueFactory>(enablePrivate: true);

            // Act
            var persistentQueue = persistentQueueFactory.Create<TestItem>("key1");

            // Assert
            persistentQueue.Should().NotBeNull();
        }
    }
}
