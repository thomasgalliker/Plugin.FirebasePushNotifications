using FluentAssertions;
using Plugin.FirebasePushNotifications.Internals;

namespace Plugin.FirebasePushNotifications.Tests.Internals
{
    public class TokenFormatterTests
    {
        [Fact]
        public void ShouldAnonymizeToken_NullToken()
        {
            // Arrange
            const string token = null;

            // Act
            var anonymizedToken = TokenFormatter.AnonymizeToken(token);

            // Assert
            anonymizedToken.Should().Be("null");
        }

        [Fact]
        public void ShouldAnonymizeToken_ValidToken()
        {
            // Arrange
            const string token = "f3480bc637d843e28a9aebab1ce3475e3563bafe05a345efb71cf4956250e290624a18a47af04ca8921c1736a3ddc953";

            // Act
            var anonymizedToken = TokenFormatter.AnonymizeToken(token);

            // Assert
            anonymizedToken.Should().Be("f3480bc63...6a3ddc953");
        }

        [Fact]
        public void ShouldAnonymizeToken_ShortToken()
        {
            // Arrange
            const string token = "f3480bc";

            // Act
            var anonymizedToken = TokenFormatter.AnonymizeToken(token);

            // Assert
            anonymizedToken.Should().Be("...");
        }
    }
}