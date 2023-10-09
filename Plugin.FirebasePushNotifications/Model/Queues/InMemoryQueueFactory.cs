namespace Plugin.FirebasePushNotifications.Model.Queues
{
    public class InMemoryQueueFactory : IQueueFactory
    {
        public IQueue<T> Create<T>()
        {
            return new InMemoryQueue<T>();
        }
    }
}