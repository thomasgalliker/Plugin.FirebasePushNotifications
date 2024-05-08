namespace Plugin.FirebasePushNotifications.Model.Queues
{
    public class InMemoryQueueFactory : IQueueFactory
    {
        public IQueue<T> Create<T>(string key)
        {
            return new InMemoryQueue<T>();
        }
    }
}