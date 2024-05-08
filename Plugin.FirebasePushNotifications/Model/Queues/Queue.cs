using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Plugin.FirebasePushNotifications.Model.Queues
{
    /// <summary>
    /// Non thread-safe implementation of an in-memory queue
    /// that implements <see cref="IQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("Queue<{typeof(T).Name,nq}> Count={this.Count}")]
    internal class Queue<T> : IQueue<T>
    {
        private readonly System.Collections.Generic.Queue<T> queue;

        public Queue()
        {
            this.queue = new System.Collections.Generic.Queue<T>();
        }

        public Queue(IEnumerable<T> collection)
        {
            this.queue = new System.Collections.Generic.Queue<T>(collection);
        }

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

        public T[] ToArray()
        {
            return this.queue.ToArray();
        }
    }
}