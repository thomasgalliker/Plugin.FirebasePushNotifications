using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Plugin.FirebasePushNotifications.Model.Queues
{
    /// <summary>
    /// Thread-safe implementation of an in-memory queue
    /// that implements <see cref="IQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("InMemoryQueue<{typeof(T).Name,nq}> Count={this.Count}")]
    internal class InMemoryQueue<T> : IQueue<T>
    {
        private readonly object lockObj = new object();
        private readonly Queue<T> queue;

        public InMemoryQueue()
        {
            this.queue = new Queue<T>();
        }

        public int Count
        {
            get
            {
                lock (this.lockObj)
                {
                    return this.queue.Count;
                }
            }
        }

        public void Clear()
        {
            lock (this.lockObj)
            {
                this.queue.Clear();
            }
        }

        public bool TryDequeue(out T item)
        {
            lock (this.lockObj)
            {
                return this.queue.TryDequeue(out item);
            }
        }

        public IEnumerable<T> TryDequeueAll()
        {
            lock (this.lockObj)
            {
                while (this.queue.TryDequeue(out var item))
                {
                    yield return item;
                }
            }
        }

        public void Enqueue(T item)
        {
            lock (this.lockObj)
            {
                this.queue.Enqueue(item);
            }
        }

        public bool TryPeek([MaybeNullWhen(false)] out T result)
        {
            lock (this.lockObj)
            {
                return this.queue.TryPeek(out result);
            }
        }

        public T[] ToArray()
        {
            lock (this.lockObj)
            {
                return this.queue.ToArray();
            }
        }
    }
}