using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Plugin.FirebasePushNotifications.Model.Queues;
using Plugin.FirebasePushNotifications.Platforms;
using Plugin.FirebasePushNotifications.Tests.Logging;
using Xunit.Abstractions;

namespace Plugin.FirebasePushNotifications.Tests
{
    public class FirebasePushNotificationManagerTests
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly AutoMocker autoMocker;

        public FirebasePushNotificationManagerTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
            this.autoMocker = new AutoMocker();

            this.autoMocker.Use<ILogger<FirebasePushNotificationManager>>(
                new TestOutputHelperLogger<FirebasePushNotificationManager>(this.testOutputHelper));

            this.autoMocker.Use<IQueueFactory>(new InMemoryQueueFactory());
        }

        [Fact]
        public void OnTokenRefresh_ShouldDeliverImmediately_IfEventIsSubscribed()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var token = "token";

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.OnTokenRefresh += (s, e) => listOfEventArgs.Add(e);

            // Act
            firebasePushNotificationManager.RaiseOnTokenRefresh(token);
            firebasePushNotificationManager.RaiseOnTokenRefresh(token);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationTokenEventArgs>();
        }

        [Fact]
        public void OnTokenRefresh_ShouldDeliverDelayed_IfEventIsSubscribedAfterDelivery()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var token = "token";

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.RaiseOnTokenRefresh(token);
            firebasePushNotificationManager.RaiseOnTokenRefresh(token);

            // Act
            firebasePushNotificationManager.OnTokenRefresh += (s, e) => listOfEventArgs.Add(e);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationTokenEventArgs>();
        }

        [Fact]
        public void OnNotificationReceived_ShouldDeliverImmediately_IfEventIsSubscribed()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var data = new Dictionary<string, object>
            {
                { "key", "value" }
            };

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.OnNotificationReceived += (s, e) => listOfEventArgs.Add(e);

            // Act
            firebasePushNotificationManager.HandleNotificationReceived(data);
            firebasePushNotificationManager.HandleNotificationReceived(data);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationDataEventArgs>();
        }

        [Fact]
        public void OnNotificationReceived_ShouldDeliverDelayed_IfEventIsSubscribedAfterDelivery()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var data = new Dictionary<string, object>
            {
                { "key", "value" }
            };

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.HandleNotificationReceived(data);
            firebasePushNotificationManager.HandleNotificationReceived(data);

            // Act
            firebasePushNotificationManager.OnNotificationReceived += (s, e) => listOfEventArgs.Add(e);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationDataEventArgs>();
        }

        [Fact]
        public void OnNotificationDeleted_ShouldDeliverImmediately_IfEventIsSubscribed()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var data = new Dictionary<string, object>
            {
                { "key", "value" }
            };

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.OnNotificationDeleted += (s, e) => listOfEventArgs.Add(e);

            // Act
            firebasePushNotificationManager.HandleNotificationDeleted(data);
            firebasePushNotificationManager.HandleNotificationDeleted(data);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationDataEventArgs>();
        }

        [Fact]
        public void OnNotificationDeleted_ShouldDeliverDelayed_IfEventIsSubscribedAfterDelivery()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var data = new Dictionary<string, object>
            {
                { "key", "value" }
            };

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.HandleNotificationDeleted(data);
            firebasePushNotificationManager.HandleNotificationDeleted(data);

            // Act
            firebasePushNotificationManager.OnNotificationDeleted += (s, e) => listOfEventArgs.Add(e);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationDataEventArgs>();
        }

        [Fact]
        public void OnNotificationOpened_ShouldDeliverImmediately_IfEventIsSubscribed()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var data = new Dictionary<string, object>
            {
                { "key", "value" }
            };
            var identifier = "99";

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.OnNotificationOpened += (s, e) => listOfEventArgs.Add(e);

            // Act
            firebasePushNotificationManager.HandleNotificationOpened(data, identifier, NotificationCategoryType.Default);
            firebasePushNotificationManager.HandleNotificationOpened(data, identifier, NotificationCategoryType.Default);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationResponseEventArgs>();
        }

        [Fact]
        public void OnNotificationOpened_ShouldDeliverDelayed_IfEventIsSubscribedAfterDelivery()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var data = new Dictionary<string, object>
            {
                { "key", "value" }
            };
            var identifier = "99";

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.HandleNotificationOpened(data, identifier, NotificationCategoryType.Default);
            firebasePushNotificationManager.HandleNotificationOpened(data, identifier, NotificationCategoryType.Default);

            // Act
            firebasePushNotificationManager.OnNotificationOpened += (s, e) => listOfEventArgs.Add(e);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationResponseEventArgs>();
        }

        [Fact]
        public void OnNotificationAction_ShouldDeliverImmediately_IfEventIsSubscribed()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var data = new Dictionary<string, object>
            {
                { "key", "value" }
            };
            var identifier = "99";

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.OnNotificationAction += (s, e) => listOfEventArgs.Add(e);

            // Act
            firebasePushNotificationManager.HandleNotificationAction(data, identifier, NotificationCategoryType.Default);
            firebasePushNotificationManager.HandleNotificationAction(data, identifier, NotificationCategoryType.Default);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationResponseEventArgs>();
        }

        [Fact]
        public void OnNotificationAction_ShouldDeliverDelayed_IfEventIsSubscribedAfterDelivery()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var data = new Dictionary<string, object>
            {
                { "key", "value" }
            };
            var identifier = "99";

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.HandleNotificationAction(data, identifier, NotificationCategoryType.Default);
            firebasePushNotificationManager.HandleNotificationAction(data, identifier, NotificationCategoryType.Default);

            // Act
            firebasePushNotificationManager.OnNotificationAction += (s, e) => listOfEventArgs.Add(e);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationResponseEventArgs>();
        }

        [Fact]
        public void OnNotificationAction_ShouldDropEvent_IfNoSubscriptionOrQueueIsPresent()
        {
            // Arrange
            var queueFactoryMock = new Mock<IQueueFactory>();
            queueFactoryMock.Setup(q => q.Create<FirebasePushNotificationDataEventArgs>())
                .Returns((IQueue<FirebasePushNotificationDataEventArgs>)null);

            this.autoMocker.Use(queueFactoryMock);

            var loggerMock = new Mock<ILogger<FirebasePushNotificationManager>>();
            this.autoMocker.Use(loggerMock.Object);

            var listOfEventArgs = new List<EventArgs>();

            var data = new Dictionary<string, object>
            {
                { "key", "value" }
            };
            var identifier = "99";

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            queueFactoryMock.Invocations.Clear();

            // Act
            firebasePushNotificationManager.HandleNotificationAction(data, identifier, NotificationCategoryType.Default);

            // Assert
            listOfEventArgs.Should().HaveCount(0);

            queueFactoryMock.VerifyNoOtherCalls();

            loggerMock.Verify(l => l.Log(
                LogLevel.Warning, 
                It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((o, t) => o.ToString() == "HandleNotificationAction has dropped an invocation of event \"OnNotificationAction\" since neither an event subscription nor a queue is present."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            loggerMock.VerifyNoOtherCalls();
        }
    }
}