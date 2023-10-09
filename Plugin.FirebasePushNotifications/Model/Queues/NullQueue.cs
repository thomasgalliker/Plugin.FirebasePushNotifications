using System.Diagnostics.CodeAnalysis;

namespace Plugin.FirebasePushNotifications.Model.Queues
{
    internal class NullQueue<T> : IQueue<T>
    {
        public int Count => 0;

        public void Clear()
        {
        }

        public void Enqueue(T item)
        {
        }

        public bool TryDequeue(out T item)
        {
            item = default;
            return false;
        }

        public IEnumerable<T> TryDequeueAll()
        {
            yield break;
        }

        public bool TryPeek([MaybeNullWhen(false)] out T result)
        {
            result = default;
            return false;
        }
    }
}