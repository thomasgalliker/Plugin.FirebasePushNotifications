namespace Plugin.FirebasePushNotifications.Model.Queues
{
    public class NullQueueFactory : IQueueFactory
    {
        public IQueue<T> Create<T>(QueueFactoryContext context)
        {
            return new NullQueue<T>();
        }
    }
}