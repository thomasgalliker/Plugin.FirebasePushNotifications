using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Plugin.FirebasePushNotifications.Tests.Logging
{
    public class TestOutputHelperLoggerFactory : ILoggerFactory
    {
        private readonly ITestOutputHelper testOutputHelper;

        public TestOutputHelperLoggerFactory(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (categoryName == null)
            {
                throw new ArgumentNullException(nameof(categoryName));
            }

            return new TestOutputHelperLogger(this.testOutputHelper, categoryName);
        }

        public void AddProvider(ILoggerProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
        }

        public void Dispose()
        {
        }
    }
}