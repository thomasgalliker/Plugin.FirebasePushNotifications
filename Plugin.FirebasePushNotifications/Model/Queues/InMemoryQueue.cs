using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Plugin.FirebasePushNotifications.Model.Queues
{
    /// <summary>
    /// Thread-safe in-memory queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("InMemoryQueue<{typeof(T).Name,nq}> Count={this.Count}")]
    internal class InMemoryQueue<T> : IQueue<T>
    {
        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        public int Count => this.queue.Count;

        public void Clear()
        {
            this.queue.Clear();
        }

        public bool TryDequeue(out T item)
        {
            return this.queue.TryDequeue(out item);
        }

        public IEnumerable<T> TryDequeueAll()
        {
            while (this.queue.TryDequeue(out var item))
            {
                yield return item;
            }
        }

        public void Enqueue(T item)
        {
            this.queue.Enqueue(item);
        }

        public bool TryPeek([MaybeNullWhen(false)] out T result)
        {
            return this.queue.TryPeek(out result);
        }
    }
}