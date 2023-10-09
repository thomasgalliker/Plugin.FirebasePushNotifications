namespace Plugin.FirebasePushNotifications.Model.Queues
{
    public interface IQueue<T>
    {
        int Count { get; }

        void Enqueue(T item);

        bool TryDequeue(out T item);
    }
}