namespace Plugin.FirebasePushNotifications.Model.Queues
{
    public class PersistentQueueFactory : IQueueFactory
    {
        public IQueue<T> Create<T>(QueueFactoryContext context)
        {
            return new PersistentQueue<T>();
        }
    }
}