using Microsoft.Extensions.Logging;

namespace Plugin.FirebasePushNotifications.Model.Queues
{
    public interface IQueueFactory
    {
        /// <summary>
        /// Creates a new queue instance.
        /// </summary>
        /// <typeparam name="T">Generic type.</typeparam>
        /// <param name="key">The key which may be used to tag the queue.</param>
        /// <param name="loggerFactory">A logger factory from which a new logger instance is created.</param>
        /// <returns>A queue instance of generic type T.</returns>
        IQueue<T> Create<T>(string key, ILoggerFactory loggerFactory);
    }
}