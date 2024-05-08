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

            this.autoMocker.Use(new FirebasePushNotificationOptions
            {
                QueueFactory = new InMemoryQueueFactory(),
                Preferences = this.autoMocker.GetMock<IFirebasePushNotificationPreferences>().Object,
            });
        }

        [Fact]
        public void OnTokenRefresh_ShouldDeliverImmediately_IfEventIsSubscribed()
        {
            // Arrange
            var firebasePushNotificationPreferences = this.autoMocker.GetMock<IFirebasePushNotificationPreferences>();

            var listOfEventArgs = new List<EventArgs>();

            var token = "test-push-token-63fd4bc9-c337-488f-bac4-13eb50e66a9c";

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.TokenRefreshed += (s, e) => listOfEventArgs.Add(e);

            // Act
            firebasePushNotificationManager.HandleTokenRefresh(token);
            firebasePushNotificationManager.HandleTokenRefresh(token);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationTokenEventArgs>();

            firebasePushNotificationPreferences.Verify(p => p.Set(Constants.Preferences.TokenKey, token), Times.Exactly(2));
            firebasePushNotificationPreferences.VerifyNoOtherCalls();
        }

        [Fact]
        public void OnTokenRefresh_ShouldDeliverDelayed_IfEventIsSubscribedAfterDelivery()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var token = "test-push-token-63fd4bc9-c337-488f-bac4-13eb50e66a9c";

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.HandleTokenRefresh(token);
            firebasePushNotificationManager.HandleTokenRefresh(token);

            // Act
            firebasePushNotificationManager.TokenRefreshed += (s, e) => listOfEventArgs.Add(e);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationTokenEventArgs>();
        }

        [Fact]
        public void OnTokenRefresh_ShouldDeliverDelayed_IfEventIsSubscribedAfterDelivery_SubscribeUnsubscribeSubscribeCase()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var token = "test-push-token-63fd4bc9-c337-488f-bac4-13eb50e66a9c";

            void OnTokenRefreshed(object s, EventArgs e)
            {
                listOfEventArgs.Add(e);
            }

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.TokenRefreshed += OnTokenRefreshed;
            firebasePushNotificationManager.TokenRefreshed -= OnTokenRefreshed;

            firebasePushNotificationManager.HandleTokenRefresh(token);
            firebasePushNotificationManager.HandleTokenRefresh(token);

            // Act
            firebasePushNotificationManager.TokenRefreshed += OnTokenRefreshed;

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationTokenEventArgs>();
        }

        [Fact]
        public void ShouldClearQueues()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var token = "test-push-token-63fd4bc9-c337-488f-bac4-13eb50e66a9c";

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.HandleTokenRefresh(token);
            firebasePushNotificationManager.HandleTokenRefresh(token);

            // Act
            firebasePushNotificationManager.ClearQueues();
            firebasePushNotificationManager.TokenRefreshed += (s, e) => listOfEventArgs.Add(e);

            // Assert
            listOfEventArgs.Should().HaveCount(0);
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
            firebasePushNotificationManager.NotificationReceived += (s, e) => listOfEventArgs.Add(e);

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
            firebasePushNotificationManager.NotificationReceived += (s, e) => listOfEventArgs.Add(e);

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
            firebasePushNotificationManager.NotificationDeleted += (s, e) => listOfEventArgs.Add(e);

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
            firebasePushNotificationManager.NotificationDeleted += (s, e) => listOfEventArgs.Add(e);

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
            firebasePushNotificationManager.NotificationOpened += (s, e) => listOfEventArgs.Add(e);

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
            firebasePushNotificationManager.NotificationOpened += (s, e) => listOfEventArgs.Add(e);

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
            firebasePushNotificationManager.NotificationAction += (s, e) => listOfEventArgs.Add(e);

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
            firebasePushNotificationManager.NotificationAction += (s, e) => listOfEventArgs.Add(e);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationResponseEventArgs>();
        }

        [Fact]
        public void OnNotificationAction_ShouldDropEvent_IfNoSubscriptionAndNoQueueIsPresent()
        {
            // Arrange
            var firebasePushNotificationPreferences = this.autoMocker.GetMock<IFirebasePushNotificationPreferences>();

            var queueFactoryMock = new Mock<IQueueFactory>();
            queueFactoryMock.Setup(q => q.Create<FirebasePushNotificationDataEventArgs>(It.IsAny<string>()))
                .Returns((IQueue<FirebasePushNotificationDataEventArgs>)null);

            this.autoMocker.Use(new FirebasePushNotificationOptions
            {
                QueueFactory = queueFactoryMock.Object,
                Preferences = firebasePushNotificationPreferences.Object,
            });

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
            loggerMock.Invocations.Clear();

            // Act
            firebasePushNotificationManager.HandleNotificationAction(data, identifier, NotificationCategoryType.Default);

            // Assert
            listOfEventArgs.Should().HaveCount(0);

            queueFactoryMock.VerifyNoOtherCalls();
            
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString() == "HandleNotificationAction drops event \"NotificationAction\" (no event subscribers / no queue present)."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}