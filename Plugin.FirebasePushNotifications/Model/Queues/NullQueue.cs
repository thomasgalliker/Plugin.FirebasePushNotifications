namespace Plugin.FirebasePushNotifications.Model.Queues
{
    internal class NullQueue<T> : IQueue<T>
    {
        public int Count => 0;

        public void Enqueue(T item)
        {
        }

        public bool TryDequeue(out T item)
        {
            item = default;
            return false;
        }
    }
}