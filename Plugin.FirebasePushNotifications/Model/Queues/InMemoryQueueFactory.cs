namespace Plugin.FirebasePushNotifications.Model.Queues
{
    public class InMemoryQueueFactory : IQueueFactory
    {
        public IQueue<T> Create<T>(QueueFactoryContext context)
        {
            return new InMemoryQueue<T>();
        }
    }
}