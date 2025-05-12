using Microsoft.Extensions.Logging;

namespace Plugin.FirebasePushNotifications.Model.Queues
{
    /// <summary>
    /// IQueueFactory which uses memory temporarily store queued objects.
    /// </summary>
    public class InMemoryQueueFactory : IQueueFactory
    {
        public IQueue<T> Create<T>(string key)
        {
            return new InMemoryQueue<T>();
        }

        public IQueue<T> Create<T>(string key, ILoggerFactory _)
        {
            return this.Create<T>(key);
        }
    }
}