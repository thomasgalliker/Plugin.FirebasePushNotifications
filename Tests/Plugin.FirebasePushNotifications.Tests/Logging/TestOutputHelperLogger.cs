using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Plugin.FirebasePushNotifications.Tests.Logging
{
    public class TestOutputHelperLogger : ILogger
    {
        private const string EndOfLine = "[EOL]";
        private readonly ITestOutputHelper testOutputHelper;
        private readonly string targetName;

        public TestOutputHelperLogger(ITestOutputHelper testOutputHelper, string targetName)
        {
            this.targetName = targetName;
            this.testOutputHelper = testOutputHelper;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                var message = formatter?.Invoke(state, exception);
                var messageLine = $"{DateTime.UtcNow} - {logLevel} - {this.targetName} - {message} {EndOfLine}";
                this.testOutputHelper.WriteLine(messageLine);
                Debug.WriteLine(messageLine);
            }
            catch (InvalidOperationException)
            {
                // TestOutputHelperLogger throws an InvalidOperationException
                // if it is no longer associated with a test case.
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state)
        {
            return new NonDisposable();
        }

        private class NonDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }

    public class TestOutputHelperLogger<T> : TestOutputHelperLogger, ILogger<T>
    {
        public TestOutputHelperLogger(ITestOutputHelper testOutputHelper)
             : base(testOutputHelper, typeof(T).Name)
        {
        }
    }
}