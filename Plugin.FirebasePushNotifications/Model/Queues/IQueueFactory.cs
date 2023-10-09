namespace Plugin.FirebasePushNotifications.Model.Queues
{
    public interface IQueueFactory
    {
        IQueue<T> Create<T>(QueueFactoryContext context);
    }
}