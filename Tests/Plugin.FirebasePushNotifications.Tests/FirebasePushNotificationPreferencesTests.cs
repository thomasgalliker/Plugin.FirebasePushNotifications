using FluentAssertions;
using Microsoft.Maui.Storage;
using Moq;
using Moq.AutoMock;

namespace Plugin.FirebasePushNotifications.Tests
{
    public class FirebasePushNotificationPreferencesTests
    {
        private readonly AutoMocker autoMocker;

        public FirebasePushNotificationPreferencesTests()
        {
            this.autoMocker = new AutoMocker();
        }

        [Fact]
        public void ShouldSet_ThrowsExceptionIfKeyIsNotRegistered()
        {
            // Arrange
            var firebasePushNotificationPreferences = this.autoMocker.CreateInstance<FirebasePushNotificationPreferences>();

            // Act
            Action action = () => firebasePushNotificationPreferences.Set("unknown_key", "value");

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ShouldSetValue_TestObject()
        {
            // Arrange
            var key = Constants.Preferences.SubscribedTopicsKey;

            var preferencesMock = this.autoMocker.GetMock<IPreferences>();
            var firebasePushNotificationPreferences = this.autoMocker.CreateInstance<FirebasePushNotificationPreferences>();
            var testObject = new TestObject
            {
                IntProperty = 99,
                DateTimeProperty = new DateTime(2000, 1, 1, 12, 13, 14, DateTimeKind.Utc),
            };

            // Act
            firebasePushNotificationPreferences.Set(key, testObject);

            // Assert
            preferencesMock.Verify(p => p.Set(key, "{\"IntProperty\":99,\"DateTimeProperty\":\"2000-01-01T12:13:14Z\"}", null), Times.Once);
            preferencesMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldGet_ThrowsExceptionIfKeyIsNotRegistered()
        {
            // Arrange
            var firebasePushNotificationPreferences = this.autoMocker.CreateInstance<FirebasePushNotificationPreferences>();

            // Act
            Action action = () => firebasePushNotificationPreferences.Get<object>("unknown_key");

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ShouldGetValue_String()
        {
            // Arrange
            var key = Constants.Preferences.TokenKey;

            var preferencesMock = this.autoMocker.GetMock<IPreferences>();
            preferencesMock.Setup(p => p.Get<string>(key, null, null))
                .Returns(() => "token_value");

            var firebasePushNotificationPreferences = this.autoMocker.CreateInstance<FirebasePushNotificationPreferences>();

            // Act
            var value = firebasePushNotificationPreferences.Get<string>(key);

            // Assert
            value.Should().Be("token_value");
            preferencesMock.Verify(p => p.Get<string>(key, null, null), Times.Once);
            preferencesMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldGetValue_TestObject()
        {
            // Arrange
            var key = Constants.Preferences.SubscribedTopicsKey;

            var preferencesMock = this.autoMocker.GetMock<IPreferences>();
            preferencesMock.Setup(p => p.Get<string>(key, null, null))
                .Returns(() => "{\"IntProperty\":99,\"DateTimeProperty\":\"2000-01-01T12:13:14Z\"}");

            var firebasePushNotificationPreferences = this.autoMocker.CreateInstance<FirebasePushNotificationPreferences>();

            // Act
            var value = firebasePushNotificationPreferences.Get<TestObject>(key);

            // Assert
            value.Should().BeEquivalentTo(new TestObject
            {
                IntProperty = 99,
                DateTimeProperty = new DateTime(2000, 1, 1, 12, 13, 14, DateTimeKind.Utc),
            });
            preferencesMock.Verify(p => p.Get<string>(key, null, null), Times.Once);
            preferencesMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldGetValue_DefaultValue()
        {
            // Arrange
            var key = Constants.Preferences.SubscribedTopicsKey;
            var defaultValue = Array.Empty<string>();

            var preferencesMock = this.autoMocker.GetMock<IPreferences>();
            preferencesMock.Setup(p => p.Get<string>(key, null, null))
                .Returns(() => "some invalid value");

            var firebasePushNotificationPreferences = this.autoMocker.CreateInstance<FirebasePushNotificationPreferences>();

            // Act
            var value = firebasePushNotificationPreferences.Get(key, defaultValue);

            // Assert
            value.Should().BeEquivalentTo(defaultValue);
            preferencesMock.Verify(p => p.Get<string>(key, null, null), Times.Once);
            preferencesMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void ShouldClearAllValues()
        {
            // Arrange
            var preferencesMock = this.autoMocker.GetMock<IPreferences>();
            var firebasePushNotificationPreferences = this.autoMocker.CreateInstance<FirebasePushNotificationPreferences>();

            // Act
            firebasePushNotificationPreferences.ClearAll();

            // Assert
            preferencesMock.Verify(p => p.Remove(It.IsAny<string>(), null), Times.Exactly(Constants.Preferences.AllKeys.Count));
            preferencesMock.VerifyNoOtherCalls();
        }

        internal class TestObject
        {
            public int IntProperty { get; set; }

            public DateTime DateTimeProperty { get; set; }
        }
    }
}
