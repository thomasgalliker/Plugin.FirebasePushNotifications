using Microsoft.Extensions.Logging;

namespace Plugin.FirebasePushNotifications.Model.Queues
{
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