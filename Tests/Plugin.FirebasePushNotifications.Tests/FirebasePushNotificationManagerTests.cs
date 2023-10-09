using FluentAssertions;
using Microsoft.Extensions.Logging;
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
        public void OnNotificationReceived_ShouldDeliverImmediately_IfEventIsSubscribed()
        {
            // Arrange
            var listOfEventArgs = new List<EventArgs>();

            var data = new Dictionary<string, object>
            {
                { "key", "value" }
            };
            var eventArgs = new FirebasePushNotificationDataEventArgs(data);

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.OnNotificationReceived += (s, e) => listOfEventArgs.Add(e);

            // Act
            firebasePushNotificationManager.RaiseOnNotificationReceived(eventArgs);
            firebasePushNotificationManager.RaiseOnNotificationReceived(eventArgs);

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
            var eventArgs = new FirebasePushNotificationDataEventArgs(data);

            var firebasePushNotificationManager = this.autoMocker.CreateInstance<TestFirebasePushNotificationManager>();
            firebasePushNotificationManager.RaiseOnNotificationReceived(eventArgs);
            firebasePushNotificationManager.RaiseOnNotificationReceived(eventArgs);

            // Act
            firebasePushNotificationManager.OnNotificationReceived += (s, e) => listOfEventArgs.Add(e);

            // Assert
            listOfEventArgs.Should().HaveCount(2);
            listOfEventArgs.Should().AllBeOfType<FirebasePushNotificationDataEventArgs>();
        }

        //[Fact]
        //public void OnOpened_ShouldDeliverImmediatelyIfEventIsSubscribed()
        //{
        //    // Arrange
        //    var listOfEventArgs = new List<EventArgs>();

        //    var data = new Dictionary<string, object>
        //    {
        //        {
        //            "key", "value"
        //        }
        //    };
        //    var eventArgs = new FirebasePushNotificationResponseEventArgs(data);

        //    var logger = new TestOutputHelperLogger(this, this.testOutputHelper);
        //    var firebasePushNotificationMock = new Mock<IFirebasePushNotification>();

        //    IPushNotificationQueue pushNotificationQueue = new PushNotificationQueue(logger, firebasePushNotificationMock.Object);
        //    pushNotificationQueue.OnNotificationReceived += (s, e) => listOfEventArgs.Add(e);
        //    pushNotificationQueue.OnNotificationOpened += (s, e) => listOfEventArgs.Add(e);

        //    // Act
        //    firebasePushNotificationMock.Raise(f => f.OnNotificationOpened += null, eventArgs);

        //    // Assert
        //    Assert.Single(listOfEventArgs);
        //    Assert.All(listOfEventArgs, e => { Assert.IsType<FirebasePushNotificationResponseEventArgs>(e); });
        //}

        //[Fact]
        //public void OnOpened_ShouldDeliverDelayedIfEventIsSubscribedAfterDelivery()
        //{
        //    // Arrange
        //    var listOfEventArgs = new List<EventArgs>();

        //    var data1 = new Dictionary<string, object>
        //    {
        //        {
        //            "key1", "value1"
        //        }
        //    };
        //    var eventArgs1 = new FirebasePushNotificationResponseEventArgs(data1);

        //    var data2 = new Dictionary<string, object>
        //    {
        //        {
        //            "key2", "value2"
        //        }
        //    };
        //    var eventArgs2 = new FirebasePushNotificationResponseEventArgs(data2);

        //    var logger = new TestOutputHelperLogger(this, this.testOutputHelper);
        //    var firebasePushNotificationMock = new Mock<IFirebasePushNotification>();

        //    IPushNotificationQueue pushNotificationQueue = new PushNotificationQueue(logger, firebasePushNotificationMock.Object);
        //    firebasePushNotificationMock.Raise(f => f.OnNotificationOpened += null, eventArgs1);
        //    firebasePushNotificationMock.Raise(f => f.OnNotificationOpened += null, eventArgs2);

        //    // Act
        //    pushNotificationQueue.OnNotificationOpened += (s, e) => listOfEventArgs.Add(e);

        //    // Assert
        //    Assert.Equal(2, listOfEventArgs.Count);
        //    Assert.All(listOfEventArgs, e => { Assert.IsType<FirebasePushNotificationResponseEventArgs>(e); });
        //}
    }
}