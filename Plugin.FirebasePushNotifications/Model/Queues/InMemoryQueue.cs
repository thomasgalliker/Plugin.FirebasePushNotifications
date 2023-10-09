using System.Collections.Concurrent;

namespace Plugin.FirebasePushNotifications.Model.Queues
{
    public class InMemoryQueue<T> : IQueue<T> //TODO: Mark internal
    {
        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        public int Count => this.queue.Count;

        public void Enqueue(T item)
        {
            this.queue.Enqueue(item);
        }

        public bool TryDequeue(out T item)
        {
            return this.queue.TryDequeue(out item);
        }
    }
}