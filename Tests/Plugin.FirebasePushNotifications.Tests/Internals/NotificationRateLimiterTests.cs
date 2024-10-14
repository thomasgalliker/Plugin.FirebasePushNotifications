using System.Diagnostics;
using FluentAssertions;
using Plugin.FirebasePushNotifications.Internals;

namespace Plugin.FirebasePushNotifications.Tests.Internals
{
    public class NotificationRateLimiterTests
    {
        private readonly TimeSpan expirationPeriod;

        public NotificationRateLimiterTests()
        {
            this.expirationPeriod = Debugger.IsAttached ?
                TimeSpan.FromMilliseconds(10000) :
                TimeSpan.FromMilliseconds(100);
        }

        [Fact]
        public async Task HasReachedLimit_ShouldReturnFalse_WhenIdentifierHasExpired()
        {
            // Arrange
            var rateLimiter = new NotificationRateLimiter();

            // Act
            var result1 = rateLimiter.HasReachedLimit("identifier1", this.expirationPeriod);
            await Task.Delay(this.expirationPeriod * 1.5);
            var result2 = rateLimiter.HasReachedLimit("identifier1", this.expirationPeriod);

            // Assert
            result1.Should().BeFalse();
            result2.Should().BeFalse();
            rateLimiter.Count.Should().Be(1);
        }

        [Fact]
        public void HasReachedLimit_ShouldReturnFalse_WhenNewIdentifierIsAdded()
        {
            // Arrange
            var rateLimiter = new NotificationRateLimiter();

            // Act
            var result1 = rateLimiter.HasReachedLimit("identifier1", this.expirationPeriod);
            var result2 = rateLimiter.HasReachedLimit("identifier2", this.expirationPeriod);

            // Assert
            result1.Should().BeFalse();
            result2.Should().BeFalse();
            rateLimiter.Count.Should().Be(1);
        }
        [Fact]
        public void HasReachedLimit_ShouldReturnTrue_WhenIdentifierIsNotExpired()
        {
            // Arrange
            var rateLimiter = new NotificationRateLimiter();

            // Act
            var result1 = rateLimiter.HasReachedLimit("identifier1", this.expirationPeriod);
            var result2 = rateLimiter.HasReachedLimit("identifier1", this.expirationPeriod);

            // Assert
            result1.Should().BeFalse();
            result2.Should().BeTrue();
            rateLimiter.Count.Should().Be(1);
        }
    }
}