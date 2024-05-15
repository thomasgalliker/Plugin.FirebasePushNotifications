using Microsoft.Extensions.Logging;

namespace Plugin.FirebasePushNotifications.Model.Queues
{
    public class InMemoryQueueFactory : IQueueFactory
    {
        public ILoggerFactory LoggerFactory { get; set; }

        public IQueue<T> Create<T>(string key)
        {
            return new InMemoryQueue<T>();
        }
    }
}