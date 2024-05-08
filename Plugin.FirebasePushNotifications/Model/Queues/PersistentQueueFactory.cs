namespace Plugin.FirebasePushNotifications.Model.Queues
{
    public class PersistentQueueFactory : IQueueFactory
    {
        private readonly PersistentQueueOptions options;

        public PersistentQueueFactory()
            : this(PersistentQueueOptions.Default)
        {
        }

        public PersistentQueueFactory(PersistentQueueOptions options)
        {
            this.options = options;
        }

        public IQueue<T> Create<T>(string key)
        {
            return new PersistentQueue<T>(key, this.options);
        }
    }
}